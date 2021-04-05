using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class HealthCheckController : ApiControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Host Is Ready";
        }
    }
}