using Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Identity.Extension.ModelConfig;

public static class ApplicationUserTokenConfiguration
{
    public static void ApplicationUserTokenConfig(this ModelBuilder builder)
    {
        builder.Entity<ApplicationUserToken>(b =>
        {
            // Composite primary key consisting of the UserId, LoginProvider and Name
            b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            // Limit the size of the composite key columns due to common DB restrictions
            b.Property(t => t.LoginProvider).HasMaxLength(3000);
            b.Property(t => t.Name).HasMaxLength(150);

            // Maps to the AspNetUserTokens table
            b.ToTable("ApplicationUserToken");
        });
    }
}