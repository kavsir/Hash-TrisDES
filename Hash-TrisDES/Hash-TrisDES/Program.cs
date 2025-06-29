using Hash_TrisDES.Data;
using Hash_TrisDES.Models;
using Hash_TrisDES.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SecurityService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var security = scope.ServiceProvider.GetRequiredService<SecurityService>();

    if (!context.Users.Any(u => u.Username == "admin"))
    {
        var salt = security.GenerateSalt();
        var encrypted = security.EncryptPassword("admin", "admin123", salt);
        context.Users.Add(new User
        {
            Username = "admin",
            Salt = salt,
            EncryptedPassword = encrypted,
            IsAdmin = true,
            FailAttempts = 0,
            IsLocked = false,
            CreatedAt = DateTime.Now
        });
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
