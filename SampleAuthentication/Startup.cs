using System;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SampleAuthentication.Activators;
using SampleAuthentication.Auth;
using SampleAuthentication.DatabaseContext;
using SampleAuthentication.Helpers;
using SampleAuthentication.Middlewares;
using SampleAuthentication.Models;
using SampleAuthentication.SeedWorks;
using SampleAuthentication.SmsSenders;
using SampleAuthentication.ViewModels;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace SampleAuthentication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureCors(services);
            ConfigureAppDependencies(services);
            ConfigureAuthentication(services);
            ConfigureAppSettingsFiles(services);

            services.AddMvc()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(1));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Policy");
            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseMvc();
        }

        #region PrivateMethods

        private static void ConfigureCors(IServiceCollection services)
        {
            // tofe
            services.AddCors(o => o.AddPolicy("Policy", corsBuilder =>
            {
                corsBuilder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            // api user claim policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser",
                    policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol,
                        Constants.Strings.JwtClaims.ApiAccess));
            });

            //services.AddCors(policy =>
            //{
            //    policy.AddPolicy("LoginPost", builder =>
            //    {
            //        builder.WithOrigins("http://localhost:4200").WithMethods("Post").WithHeaders("accept", "content-type", "origin");
            //    });

            //    policy.AddPolicy("Post", builder =>
            //    {
            //        builder.WithOrigins("http://localhost:4200").WithMethods("Post").WithHeaders("accept", "content-type");
            //    });

            //    policy.AddPolicy("Get", builder =>
            //    {
            //        builder.WithOrigins("http://localhost:4200").WithMethods("Get").WithHeaders("Authorization", "content-type");
            //    });
            //});

            //services.AddAuthorization(auth =>
            //{
            //    auth.AddPolicy("SuperAdmin", new AuthorizationPolicyBuilder().RequireRole("SuperAdmin").AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());
            //    auth.AddPolicy("Customer", new AuthorizationPolicyBuilder().RequireRole("Customer").AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build());
            //});

        }

        private void ConfigureAppDependencies(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<IJwtFactory, JwtFactory>();
            services.AddScoped<IHttpClientHelper, HttpClientHelper>();
            services.AddScoped<ISmsSender, SmsSender>();

            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = Configuration.GetSection("JwtIssuerOptions");

            SymmetricSecurityKey signingKey =
                new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)]));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SecretKey = jwtAppSettingOptions[nameof(JwtIssuerOptions.SecretKey)];
                options.ValidTime =
                    TimeSpan.FromMinutes(double.Parse(jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidTime)]));
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            // add identity
            var builder = services.AddIdentity<User, Role>(o =>
            {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
                o.Tokens.ChangePhoneNumberTokenProvider = "Phone";
            });
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions =>

             {
                 configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                 configureOptions.TokenValidationParameters = tokenValidationParameters;
                 configureOptions.SaveToken = true;
             });

            
         
        }

        private void ConfigureAppSettingsFiles(IServiceCollection services)
        {
            services.Configure<HostAddresses>(Configuration.GetSection(nameof(HostAddresses)));
        }

        #endregion
    }
}