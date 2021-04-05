using System.Collections.Generic;
using System.Security.Claims;
using SampleAuthentication.Models;

namespace SampleAuthentication.Auth
{
	public interface IJwtFactory
	{
		string GenerateEncodedToken(User user, IReadOnlyCollection<string> userRoles, IEnumerable<string> roleIds, ClaimsIdentity identity);
		ClaimsIdentity GenerateClaimsIdentity(string userName, string id);
	}
}
