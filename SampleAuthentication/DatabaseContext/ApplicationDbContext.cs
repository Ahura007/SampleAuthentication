using System.IO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SampleAuthentication.Models;

namespace SampleAuthentication.DatabaseContext
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {

        //Add-Migration Init -Verbose
       // Update-Database -Verbose

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public ApplicationDbContext()
        {
        }


        public DbSet<Role> ApplicationRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Don't remove these codes . 
            // We put these codes here because of migration problem
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }
    }
}