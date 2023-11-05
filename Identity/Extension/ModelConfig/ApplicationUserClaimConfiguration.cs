using Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Identity.Extension.ModelConfig;

public static class ApplicationUserClaimConfiguration
{
    public static void ApplicationUserClaimConfig(this ModelBuilder builder)
    {
        builder.Entity<ApplicationUserClaim>(b =>
        {
            // Primary key
            b.HasKey(uc => uc.Id);

            // Maps to the AspNetUserClaims table
            b.ToTable("ApplicationUserClaim");
        });
    }
}