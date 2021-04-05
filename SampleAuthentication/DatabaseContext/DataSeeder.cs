using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleAuthentication.Enums;
using SampleAuthentication.Extensions;
using SampleAuthentication.Models;

namespace SampleAuthentication.DatabaseContext
{
    public static class DataSeeder
    {
        public static IWebHost Seed(this IWebHost host)
        {
            try
            {
                using (var scope = host.Services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;

                    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                    context.Database.Migrate();

                    CreateRolesIfNotExists(context);
                    CreateSuperAdminIfNotExists(userManager);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            return host;
        }

        #region PrivateMethods

        private static void CreateRolesIfNotExists(ApplicationDbContext context)
        {
            var roles = EnumHelper.GetValues<RoleType>();

            foreach (var role in roles)
            {
                if (!context.Roles.Any(q => q.Name == role.ToString()))
                    context.ApplicationRoles.Add(new Role(role.ToString())
                    {
                        NormalizedName = role.ToString()
                    });
            }

            context.SaveChanges();
        }

        private static void CreateSuperAdminIfNotExists(UserManager<User> userManager)
        {
            const string username = "Administrator";
            const string password = "adminPassword";
            const string name = "کاربر ارشد";

            if (userManager.FindByNameAsync(username).Result == null)
            {
                var superAdmin = new User(username, name) { Id = Guid.NewGuid().ToString(), UserName = username, PhoneNumberConfirmed = true };
                var result = userManager.CreateAsync(superAdmin, password).Result;
                if (result.Succeeded)
                    userManager.AddToRoleAsync(superAdmin, RoleType.SuperAdmin.ToString()).Wait();
            }
        }

        #endregion
    }
}