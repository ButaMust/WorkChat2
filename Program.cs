using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkChat2.Data;
using WorkChat2.Models;

namespace WorkChat2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1) DbContext for EF Core (uses DefaultConnection from appsettings.json)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2) Identity: AppUser + Roles, stored in AppDbContext
            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            // 3) MVC + Razor Pages (Identity UI uses Razor Pages)
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // 4) Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Identity UI endpoints (/Identity/Account/Login, etc.)
            app.MapRazorPages();

            // Optional: redirect root ("/") to login if user not authenticated
            app.MapGet("/", async context =>
            {
                if (!context.User.Identity?.IsAuthenticated ?? false)
                {
                    context.Response.Redirect("/Identity/Account/Login");
                }
                else
                {
                    context.Response.Redirect("/Home/Index");
                }
            });

            // MVC route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
