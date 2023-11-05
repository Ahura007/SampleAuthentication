using Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Identity.Extension.ModelConfig;

public static class ApplicationRoleClaimConfiguration
{
    public static void ApplicationRoleClaimConfig(this ModelBuilder builder)
    {
        builder.Entity<ApplicationRoleClaim>(b =>
        {
            // Primary key
            b.HasKey(rc => rc.Id);

            // Maps to the AspNetRoleClaims table
            b.ToTable("ApplicationRoleClaim");
        });
    }
}