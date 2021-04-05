using System;
using System.Security.Claims;
using SampleAuthentication.Models;

namespace SampleAuthentication.SeedWorks
{
    public static class TokenExtensions
    {
        public static UserInfo GetUserInfo(this ClaimsIdentity claimsIdentity)
        {
            return new UserInfo
            {
                Id = Guid.Parse(claimsIdentity.FindFirst("id").Value)
            };
        }
    }
}
