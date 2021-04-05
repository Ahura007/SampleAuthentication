using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using SampleAuthentication.Auth;
using SampleAuthentication.Dtos;
using SampleAuthentication.Enums;
using SampleAuthentication.Models;
using SampleAuthentication.ViewModels;

namespace SampleAuthentication.Helpers
{
    public class TokenGenerator
    {
        public static GenerateJsonWebTokenDto EmptyToken = new GenerateJsonWebTokenDto();
        public static GenerateJsonWebTokenDto GenerateJwt(User user, IReadOnlyCollection<string> userRoles, IReadOnlyCollection<string> userRoleIds, ClaimsIdentity identity,
            IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings)
        {
            var response = new GenerateJsonWebTokenDto
            {
                name = user.Name.Replace("|", " "),
                userName = user.UserName,
                token_type = "Bearer",
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = jwtFactory.GenerateEncodedToken(user, userRoles, userRoleIds, identity),
                expires_in = (int)jwtOptions.ValidTime.TotalSeconds,
                userType = string.Join(',', userRoles),
                isSuperAdmin = userRoles.Any(q => q.Equals(RoleType.SuperAdmin.ToString())),
                first_login = user.TotalLoginCount == 1,
                roleIds = userRoleIds,
            };
            return response;
        }
    }
}