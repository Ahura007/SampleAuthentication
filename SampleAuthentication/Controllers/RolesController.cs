using System;
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
using SampleAuthentication.Extensions;
using SampleAuthentication.Helpers;
using SampleAuthentication.Models;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<Role> _roleManager;

        public RolesController(ApplicationDbContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageQuery query, [FromQuery] string search)
        {
            var roles = await SearchBy(search)
                .ToPagingAndSorting(query)
                .ToListAsync();

            var rolesDto = roles.Select(q => new RoleDto
            {
                Id = q.Id,
                Name = q.Name,
                CreationDate = q.CreationDate.FaDate(),
                TotalUsersInRoleCount = _context.UserRoles.Count(i => i.RoleId == q.Id)
            });

            return Ok(rolesDto.ToPaging(await SearchBy(search).CountAsync(), query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return StatusCode((int)HttpStatusCode.NotFound, new { message = "نقش یافت نشد" });

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name
            };

            return Ok(new { content = roleDto });
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(AddRoleCommand command)
        {
            try
            {
                CommandValidator.Validate(command);

                var roleAlreadyExists = await _roleManager.FindByNameAsync(command.Name) != null;
                if (roleAlreadyExists)
                    return StatusCode((int)HttpStatusCode.Conflict, new { message = "نام وارد شده تکراری می باشد" });

                var newRole = new Role(command.Name);

                await _roleManager.CreateAsync(newRole);

                return Ok(new { content = newRole.Id, message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, UpdateRoleCommand command)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "نقش یافت نشد" });

                role.Name = command.Name;
                await _roleManager.UpdateAsync(role);

                return Ok(new { message = ApiMessages.Ok });
            }
            catch (CommandValidationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role is null)
                    return StatusCode((int)HttpStatusCode.NotFound, new { message = "نقش یافت نشد" });

                var anyUserHasThisRole = await _context.UserRoles.AnyAsync(q => q.RoleId == role.Id);
                if (anyUserHasThisRole)
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new { message = "به دلیل وجود کاربر با این نقش ، امکان حذف وجود ندارد" });

                await _roleManager.DeleteAsync(role);
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
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "متاسفانه خطای سیستمی رخ داده است" });
            }
        }

        #region PrivateMethods

        private IQueryable<Role> SearchBy(string search)
        {
            var roles = GetNonDefaultRoles();

            if (!string.IsNullOrEmpty(search))
                roles = roles.Where(q => q.Name.Contains(search));

            return roles.OrderByDescending(q => q.Name);
        }

        private IQueryable<Role> GetNonDefaultRoles()
        {
            return _context
                .Roles.OfType<Role>()
                .Where(q => q.Name != RoleType.Customer.ToString() && q.Name != RoleType.SuperAdmin.ToString());
        }

        #endregion
    }
}