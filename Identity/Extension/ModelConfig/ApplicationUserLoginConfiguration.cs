using Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Identity.Extension.ModelConfig;

public static class ApplicationUserLoginConfiguration
{
    public static void ApplicationUserLoginConfig(this ModelBuilder builder)
    {
        builder.Entity<ApplicationUserLogin>(b =>
        {
            // Composite primary key consisting of the LoginProvider and the key to use
            // with that provider
            b.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            // Limit the size of the composite key columns due to common DB restrictions
            b.Property(l => l.LoginProvider).HasMaxLength(128);
            b.Property(l => l.ProviderKey).HasMaxLength(128);

            // Maps to the AspNetUserLogins table
            b.ToTable("ApplicationUserLogin");
        });
    }
}