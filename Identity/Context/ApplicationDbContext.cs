using Identity.Extension.ModelConfig;
using Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Context;

 
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplicationUserConfig();
        builder.ApplicationUserClaimConfig();
        builder.ApplicationUserLoginConfig();
        builder.ApplicationUserTokenConfig();
        builder.ApplicationRoleConfig();
        builder.ApplicationRoleClaimConfig();
        builder.ApplicationUserRoleConfig();

        base.OnModelCreating(builder);
    }
}





