using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using SampleAuthentication.Enums;
using SampleAuthentication.Helpers;
using SampleAuthentication.Models;
using SampleAuthentication.ViewModels;

namespace SampleAuthentication.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public string GenerateEncodedToken(User user, IReadOnlyCollection<string> userRoles, IEnumerable<string> roleIds, ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, string.Join(',', userRoles)));
            identity.AddClaim(new Claim("givenName", user.Name.Replace("|", " ")));
            identity.AddClaim(new Claim("roleIds", string.Join(',', roleIds)));
            identity.AddClaim(new Claim("isSuperAdmin", userRoles.Any(q => q.Equals(RoleType.SuperAdmin.ToString())).ToString()));
            identity.AddClaim(new Claim("userType", userRoles.Any(q => q.Equals(RoleType.Customer.ToString())) ? "Customer" : "SystemUser"));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64));

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: identity.Claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public ClaimsIdentity GenerateClaimsIdentity(string userName, string id)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Constants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess)
            });
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidTime <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidTime));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}
