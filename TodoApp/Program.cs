using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Connect DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 👇 Seed user (only in development for now)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByEmailAsync("admin@todo.com");
        if (existingUser == null)
        {
            var user = new User
            {
                UserName = "admin@todo.com",
                Email = "admin@todo.com",
                FullName = "Admin User"
            };

            await userManager.CreateAsync(user, "Admin@123"); // password must meet Identity rules
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();  // 👈 must be added before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();
//app.MapControllerRoute(
//    name: "todos",
//    pattern: "Todos/{action=Index}/{id?}",
//    defaults: new { controller = "Todos" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
