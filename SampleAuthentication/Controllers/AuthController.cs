using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SampleAuthentication.Auth;
using SampleAuthentication.Commands;
using SampleAuthentication.Dtos;
using SampleAuthentication.Helpers;
using SampleAuthentication.Models;
using SampleAuthentication.SeedWorks;
using SampleAuthentication.SmsSenders;
using SampleAuthentication.ViewModels;

namespace SampleAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly RoleManager<Role> _roleManager;
        private readonly ISmsSender _smsSender;
        private readonly UserManager<User> _userManager;

        public AuthController(UserManager<User> userManager, RoleManager<Role> roleManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions,
            ISmsSender smsSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _smsSender = smsSender;
            _configuration = configuration;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserCommand command)
        {
            CommandValidator.Validate(command);

            var user = await GetUserByUserName(command.UserName);
            CheckPhoneNumberConfirmation(user);

            if (!await CheckFundUserIsActive(user))
                return StatusCode((int) HttpStatusCode.BadRequest, new {message = "شما مجوز ورود به سامانه را ندارید"});

            var generatedToken = TokenGenerator.EmptyToken;
            var isTwoFactorEnabled = await CheckIfTwoFactorIsEnabled(user);
            if (isTwoFactorEnabled)
                await SendTwoFactorTokenToUser(user);
            else
                generatedToken = await Login(user);
            return new OkObjectResult(new
                {Token = generatedToken, TwoFactorEnabled = isTwoFactorEnabled, UserId = user.Id});
        }


        [HttpPost]
        [Route("/api/users/{userId}/verify/two-factor-verification")]
        public async Task<IActionResult> VerifyTwoFactorVerification(Guid userId,
            VerifyTwoFactorVerificationCommand command)
        {
            CommandValidator.Validate(command);
            var user = await GetUserById(userId);
            CheckPhoneNumberConfirmation(user);
            await CheckTwoFactorVerification(command, user);
            var accessToken = await Login(user);
            return new OkObjectResult(accessToken);
        }


        #region PrivateMethods

        private async Task<GenerateJsonWebTokenDto> Login(User user)
        {
            await SaveLastLoginInfo(user);
            var generatedToken = await GenerateJsonWebToken(user);
            return generatedToken;
        }

        private async Task<User> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new ArgumentException("کاربر یافت نشد");
            return user;
        }

        private async Task<User> GetUserById(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new ArgumentException("کاربر یافت نشد");
            return user;
        }

        /*
        private void CheckCaptchaValidation(string captchaId, string captchaCode)
        {
            if (_authenticationSettings.DevelopmentMode)
            {
                // a.ammari : در حالت توسعه، برای دریافت توکن توسط پُستمن و عمیات تست
                // از یک شناسه و کد کپچای از پیش تعریف شده و ثابت استفاده می کنیم.
                if (string.Equals(_authenticationSettings.DevelopmentCaptchaId, captchaId) &&
                    string.Equals(_authenticationSettings.DevelopmentCaptchaCode, captchaCode))
                {
                    return;
                }
            }
            _captchaManager.ValidateCaptcha(captchaId, captchaCode);
        }
        */

        private void CheckPhoneNumberConfirmation(User user)
        {
            //if (!user.PhoneNumberConfirmed)
            //    throw new ArgumentException("حساب کاربری شما فعال نشده است");
        }

        private async Task CheckUserPassword(User user, string password)
        {
            if (!await _userManager.CheckPasswordAsync(user, password))
                throw new ArgumentException("نام کاربری یا کلمه عبور اشتباه می باشد");
        }

        private async Task<bool> CheckFundUserIsActive(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            return userRoles.Any(q => q != "FundUser") || user.IsActive;
        }

        private async Task SendTwoFactorTokenToUser(User user)
        {
            var twoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            await _smsSender.SendSmsAsync($"کد تایید شما عبارت است از: {twoFactorToken}", user.PhoneNumber);
        }

        private async Task<bool> CheckIfTwoFactorIsEnabled(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Customer"))
                return bool.Parse(_configuration.GetSection("HasTwoFactorLoginForCustomers").Value);
            return user.TwoFactorEnabled;
        }

        private async Task SaveLastLoginInfo(User user)
        {
            user.TotalLoginCount += 1;
            user.LastLoginDateTime = DateTime.Now;

            await _userManager.UpdateAsync(user);
        }

        private async Task<GenerateJsonWebTokenDto> GenerateJsonWebToken(User user)
        {
            var identity = await GetClaimsIdentity(user.UserName, user.Id);
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRoleIds = GetUserRoleIds(userRoles);
            var generatedToken = TokenGenerator.GenerateJwt(user, userRoles.ToList(), userRoleIds.ToList(), identity,
                _jwtFactory, _jwtOptions, new JsonSerializerSettings {Formatting = Formatting.Indented});
            return generatedToken;
        }

        private IEnumerable<string> GetUserRoleIds(IEnumerable<string> userRoles)
        {
            return userRoles.Select(userRole => _roleManager.GetRoleIdAsync(new Role(userRole)).Result).ToList();
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string userId)
        {
            var identity = await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userId));
            if (identity == null)
                throw new ArgumentException("نام کاربری یا کلمه عبور اشتباه می باشد");
            return identity;
        }

        private async Task CheckTwoFactorVerification(VerifyTwoFactorVerificationCommand command, User user)
        {
            var isVerified = await _userManager.VerifyTwoFactorTokenAsync(user, "Phone", command.VerifyCode);
            if (!isVerified)
                throw new AggregateException("کد تایید نامعتبر است");
        }

        #endregion
    }
}