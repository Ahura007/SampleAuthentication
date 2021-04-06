using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAuthentication.Helpers;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Controllers.Test
{
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdmin")]
    [ApiController]
    public class SuperAdminController : ApiControllerBase
    {
        public async Task<IActionResult> Ok(string userId)
        {
            return Ok(new { message = ApiMessages.Ok });
        }
    }
}