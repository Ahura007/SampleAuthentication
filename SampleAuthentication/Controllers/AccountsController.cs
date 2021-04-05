using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SampleAuthentication.Commands;
using SampleAuthentication.Enums;
using SampleAuthentication.Helpers;
using SampleAuthentication.Models;
using SampleAuthentication.SeedWorks;
using SampleAuthentication.SmsSenders;

namespace SampleAuthentication.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : ApiControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsSender;
        private readonly UserManager<User> _userManager;

        public AccountsController(
            UserManager<User> userManager,
            ISmsSender smsSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _smsSender = smsSender;
            _configuration = configuration;
        }

        [HttpPost("RegisterCustomers")]
        public async Task<IActionResult> RegisterCustomers(RegisterCustomerCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var customerAlreadyRegistered = await _userManager.FindByNameAsync(command.UserName) != null;
                if (customerAlreadyRegistered)
                    return StatusCode((int) HttpStatusCode.Conflict,
                        new {message = "نام کاربری وارد شده تکراری می باشد"});
                var newCustomer = new User(command.UserName, command.Name)
                {
                    Id = command.Id.ToString(),
                    PhoneNumber = command.PhoneNumber,
                    NormalizedUserName = command.UserName,
                    PhoneNumberConfirmed = false
                };

                await _userManager.CreateAsync(newCustomer, command.Password);
                await _userManager.AddToRoleAsync(newCustomer, RoleType.Customer.ToString());

                var code = await _userManager.GenerateChangePhoneNumberTokenAsync(newCustomer, command.PhoneNumber);
                await _smsSender.SendSmsAsync(PrepareActivationCodeSms(code), command.PhoneNumber);

                Console.WriteLine($"{command.UserName} : {code}");

                return Ok(new {message = newCustomer.Id});
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpGet("{userName}/exists")]
        public async Task<IActionResult> IsUserExists(string userName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return NotFound();

                return Ok();
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpPost("{userId}/verificationCode/verify")]
        public async Task<IActionResult> VerifyVerificationCode(Guid userId, VerifyVerificationCodeCommand command)
        {
            CommandValidator.Validate(command);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new
                {
                    message = "کاربر یافت نشد"
                });

            var verificationCodeIsValid =
                await _userManager.VerifyChangePhoneNumberTokenAsync(user, command.Code, user.PhoneNumber);
            if (verificationCodeIsValid)
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);

                return Ok();
            }

            return BadRequest(new
            {
                message = "کد وارد شده اشتباه می باشد"
            });
        }

        [HttpPost("{userId}/verificationCode/resend")]
        public async Task<IActionResult> ResendVerificationCode(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new
                {
                    message = "کاربر یافت نشد"
                });

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            await _smsSender.SendSmsAsync(PrepareActivationCodeSms(code), user.PhoneNumber);
            Console.WriteLine($"{user.UserName} : {code}");

            return Ok();
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordCommand command)
        {
            CommandValidator.Validate(command);

            var user = await _userManager.FindByNameAsync(command.UserName);
            if (user == null)
                return NotFound(new
                {
                    message = "کاربر یافت نشد"
                });

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            await _smsSender.SendSmsAsync(PrepareForgetPasswordSms(code), user.PhoneNumber);
            Console.WriteLine($"{user.UserName} : {code}");

            return Ok(new {message = user.Id});
        }

        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid userId, ResetPasswordCommand command)
        {
            CommandValidator.Validate(command);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new
                {
                    message = "کاربر یافت نشد"
                });

            var verificationCodeIsValid =
                await _userManager.VerifyChangePhoneNumberTokenAsync(user, command.Code, user.PhoneNumber);
            if (verificationCodeIsValid)
            {
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, command.NewPassword);

                return Ok();
            }

            return BadRequest(new
            {
                message = "کد وارد شده اشتباه می باشد"
            });
        }

        [HttpPut("{userId}/password/default")]
        public async Task<IActionResult> NewPassword(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new
                {
                    mesmessage = "کاربر یافت نشد"
                });

            var defaultPassword = _configuration.GetSection("DefaultPassword").Value;
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, defaultPassword);

            return Ok(new {message = ApiMessages.Ok});
        }

        [HttpPut("{userId}/password/old")]
        public async Task<IActionResult> RecoverPassword(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new
                {
                    mesmessage = "کاربر یافت نشد"
                });

            await _userManager.RemovePasswordAsync(user);

            user.PasswordHash = user.OldPassword;

            await _userManager.UpdateAsync(user);

            return Ok(new {message = ApiMessages.Ok});
        }

        [Authorize]
        [HttpPut("password/change")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                if (!IsConfirmPassword(command.NewPassword, command.ConfirmPassword))
                    return BadRequest(new
                    {
                        message = "رمز عبور جدید و رمز تأییده شما مطابقت ندارد"
                    });

                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                    return NotFound(new {message = "کاربری یافت نشد"});

                var operationSucceeded =
                    await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
                if (operationSucceeded.Succeeded)
                    return Ok(new {message = ApiMessages.Ok});

                return BadRequest(new
                {
                    message = "کلمه عبور فعلی اشتباه می باشد"
                });
            }
            catch (Exception)
            {
                return StatusCode((int) HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("{userId}/ResetPasswordByAdmin")]
        public async Task<IActionResult> ResetPasswordByAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            await _userManager.RemovePasswordAsync(user);

            var newRandomPassword = Guid.NewGuid().ToString().Substring(0, 6);
            await _userManager.AddPasswordAsync(user, newRandomPassword);

            await _smsSender.SendSmsAsync($"مشتری گرامی . کلمه عبور جدید شما برابر است با : {newRandomPassword}",
                user.PhoneNumber);

            return Ok(new {message = ApiMessages.Ok});
        }

        #region PrivateMethods

        private string PrepareActivationCodeSms(string code)
        {
            return $"کد تایید شما عبارت است از: {code}";
        }

        private string PrepareForgetPasswordSms(string code)
        {
            return $"کد تغییر رمز عبور: {code}";
        }

        private bool IsConfirmPassword(string passWord, string reEnterPassword)
        {
            if (passWord.Equals(reEnterPassword))
                return true;

            return false;
        }

        #endregion
    }
}