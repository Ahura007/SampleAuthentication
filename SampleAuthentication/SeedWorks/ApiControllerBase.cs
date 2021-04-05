using System;
using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SampleAuthentication.SeedWorks
{
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        protected virtual Guid UserId
        {
            get
            {
                try
                {
                    var claims = HttpContext.User.Identity as ClaimsIdentity;
                    var id = Guid.Parse(claims.FindFirst("id").Value);
                    if (id != null)
                        return id;

                    throw new AuthenticationException();
                }
                catch (Exception ex)
                {
                    if (this.ControllerContext.HttpContext.User.Claims == null)
                        throw new Exception("دسترسی غیر معتبر، دوباره تلاش کنید.");
                    throw new AuthenticationException(ex.Message);
                }
            }
        }
    }
}
