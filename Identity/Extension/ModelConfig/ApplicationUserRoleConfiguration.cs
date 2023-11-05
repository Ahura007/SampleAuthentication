using Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Identity.Extension.ModelConfig;

public static class ApplicationUserRoleConfiguration
{
    public static void ApplicationUserRoleConfig(this ModelBuilder builder)
    {
        builder.Entity<ApplicationUserRole>(b =>
        {
            // Primary key
            b.HasKey(r => new { r.UserId, r.RoleId });

            // Maps to the AspNetUserRoles table
            b.ToTable("ApplicationUserRole");
        });
    }
}