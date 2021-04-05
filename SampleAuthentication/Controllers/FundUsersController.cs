using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleAuthentication.Commands;
using SampleAuthentication.DatabaseContext;
using SampleAuthentication.Dtos;
using SampleAuthentication.Enums;
using SampleAuthentication.Helpers;
using SampleAuthentication.Models;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundUsersController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public FundUsersController(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageQuery query, [FromQuery] string search, bool? isActive)
        {
            try
            {
                var users = await GetFundUsersAsync(query, search, isActive);

                var usersDto = users.Select(MapToDto);

                return Ok(usersDto.ToPaging(await CountFundUsersAsync(search, isActive), query));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                var userDto = MapToDto(user);

                return Ok(new { content = userDto });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var userAlreadyRegistered = await _userManager.FindByNameAsync(command.UserName) != null;
                if (userAlreadyRegistered)
                    return StatusCode((int)HttpStatusCode.Conflict, new { message = "نام کاربری وارد شده تکراری می باشد" });

                var user = new User(command.UserName, $"{command.FirstName}|{command.LastName}")
                {
                    PhoneNumber = command.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    Email = command.Email,
                    TwoFactorEnabled = command.TwoFactorEnabled
                };

                await RegisterUser(command, user);
                return Ok(new { content = user.Id, message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                if (!command.IsActive)
                    await DeActiveFundUser(user, id);

                user.Name = $"{command.FirstName}|{command.LastName}";
                user.PhoneNumber = command.PhoneNumber;
                user.TwoFactorEnabled = command.TwoFactorEnabled;
                user.IsActive = command.IsActive;
                await _userManager.UpdateAsync(user);

                return Ok(new { content = user.Id, message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRoles(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                var roles = await GetRolesAsync();
                var userRolesDto = new List<UserRoleDto>();

                roles.ForEach(role =>
                {
                    userRolesDto.Add(new UserRoleDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsInRole = _userManager.IsInRoleAsync(user, role.Name).Result
                    });
                });

                return Ok(new { content = userRolesDto });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRole(string id, AssignRoleCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                var userRoles = await _userManager.GetRolesAsync(user);

                await _userManager.RemoveFromRolesAsync(user, userRoles);
                foreach (var roleId in command.RoleIds)
                {
                    _context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        RoleId = roleId,
                        UserId = user.Id
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpDelete]
        [Route("{id}/roles")]
        public async Task<IActionResult> UnAssignRole(string id, UnAssignRoleCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                var role = await _roleManager.FindByIdAsync(command.RoleId);
                if (role is null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "نقش مورد نظر یافت نشد" });

                await _userManager.RemoveFromRoleAsync(user, role.Name);

                return Ok(new { message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPut]
        [Route("{userId}/active")]
        public async Task<IActionResult> Active(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                user.Active();
                await _userManager.UpdateAsync(user);

                return Ok(new { message = ApiMessages.Ok });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPut]
        [Route("{userId}/deActive")]
        public async Task<IActionResult> DeActive(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "کاربر مورد نظر یافت نشد" });

                await DeActiveFundUser(user, userId);

                await _userManager.UpdateAsync(user);

                return Ok(new { message = ApiMessages.Ok });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        #region PrivateMethods

        private async Task DeActiveFundUser(User user, string id)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any(q => q == "SuperAdmin"))
                throw new ArgumentException("امکان غیر فعال شدن این کاربر وجود ندارد");

            if (UserId.ToString() == id)
                throw new ArgumentException("امکان غیر فعال شدن این کاربر وجود ندارد");

            user.DeActive();
        }

        private async Task RegisterUser(RegisterUserCommand command, User user)
        {
            await _userManager.CreateAsync(user, command.Password);
            await _userManager.AddToRolesAsync(user, new[] { RoleType.FundUser.ToString() });
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.Name.Contains('|') ? user.Name.Split('|')[0] : user.Name,
                LastName = user.Name.Contains('|') ? user.Name.Split('|')[1] : user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = _userManager.GetRolesAsync(user).Result,
                TwoFactorEnabled = user.TwoFactorEnabled
            };
        }

        private async Task<List<User>> GetFundUsersAsync(PageQuery query, string search, bool? isActive)
        {
            var fundUsers = await _userManager.GetUsersInRoleAsync(RoleType.FundUser.ToString());

            if (!string.IsNullOrEmpty(search))
                fundUsers = FilterUsers(fundUsers, search);

            if (isActive != null)
                fundUsers = FilterActivation(fundUsers, isActive);

            return fundUsers
                .OrderBy(q => q.Name.Contains('|') ? q.Name.Split('|')[1] : q.Name)
                .ThenBy(q => q.Name.Contains('|') ? q.Name.Split('|')[0] : q.Name)
                .ToPagingAndSorting(query)
                .ToList();
        }

        private async Task<int> CountFundUsersAsync(string search, bool? isActive)
        {
            var systemUsers = await _userManager.GetUsersInRoleAsync(RoleType.FundUser.ToString());

            if (!string.IsNullOrEmpty(search))
                systemUsers = FilterUsers(systemUsers, search);

            if (isActive != null)
                systemUsers = FilterActivation(systemUsers, isActive);

            return systemUsers.Count;
        }

        private static IList<User> FilterUsers(IEnumerable<User> systemUsers, string search)
        {
            return systemUsers.Where(q =>
                   q.Name.Contains(search) ||
                    q.UserName.Contains(search) ||
                    q.PhoneNumber.Contains(search))
                .ToList();
        }

        private static IList<User> FilterActivation(IEnumerable<User> systemUsers, bool? isActive)
        {
            return systemUsers.Where(q => q.IsActive == isActive).ToList();
        }

        private Task<List<Role>> GetRolesAsync()
        {
            return _context.ApplicationRoles
                .ToListAsync();
        }

        #endregion
    }
}