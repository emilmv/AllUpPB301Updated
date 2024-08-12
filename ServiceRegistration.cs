using AllUp.Data;
using AllUp.Interfaces;
using AllUp.Models;
using AllUp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AllUp;

public static class ServiceRegistration
{
    public static void Register(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllersWithViews();
        services.AddDbContext<AllUpDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        services.AddSession();
        services.AddHttpContextAccessor();
        services.AddScoped<ILayoutService, LayoutService>();
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 7;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
        }).AddEntityFrameworkStores<AllUpDbContext>().AddDefaultTokenProviders();
    }
}
