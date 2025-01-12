using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RazorPagesAuthenticationViaWebApi.Data;
using System.Data;
using System.Globalization;

namespace RazorPagesAuthenticationViaWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseAntiforgery();

            CreateDatabaseIfNotExists(app.Services);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();

                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.MapGet("/api/LoginUser", async (string username, string password, SignInManager<IdentityUser> signInManager, IHttpContextAccessor httpContextAccessor) =>
            {
                var result = await signInManager.PasswordSignInAsync(username, password, false, false);

                if (result.Succeeded)
                {
                    //IsLoggedIn is true here
                    var IsLoggedIn = httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated;

                    return Results.Ok();
                }
                return Results.BadRequest($"SignInResult: {result}");
            });

            app.Run();
        }




        private static void CreateDatabaseIfNotExists(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    using (var context = services.GetRequiredService<ApplicationDbContext>())
                    using (var userManager = services.GetRequiredService<UserManager<IdentityUser>>())
                    {
                        var migrations = context.Database.GetPendingMigrations().ToList();
                        if (migrations.Count != 0)
                        {
                            var migrator = context.Database.GetService<IMigrator>();
                            foreach (var migration in migrations)
                            {
                                migrator.Migrate(migration);
                            }

                            //test user creation
                            var testUser = new IdentityUser("test@gmail.com") { Email = "test@gmail.com", EmailConfirmed = true };
                            var result = userManager.CreateAsync(testUser, "!TestPassword123").GetAwaiter().GetResult();
                        }
                    }

                }
                catch (Exception exception)
                {
                    logger.LogError(exception.Message);
                }
            }
        }
    }
}
