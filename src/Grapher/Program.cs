using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Grapher.Data;
using Grapher.Models;
using Grapher.Services;
using Grapher.Configuration;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Bind AppRoles from configuration and provide safe defaults if missing.
builder.Services.Configure<AppRoles>(opts =>
{
    opts.AdminRole = builder.Configuration["AppRoles:AdminRole"] ?? "Administrator";
    opts.MemberRole = builder.Configuration["AppRoles:MemberRole"] ?? "Member";
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Register SMTP configuration and email sender
builder.Services.Configure<Grapher.Services.SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<Grapher.Services.IEmailSender, Grapher.Services.SmtpEmailSender>();
// Register adapter that delegates to Grapher.Services.IEmailSender
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Grapher.Services.IdentityEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authenticate requests so User is populated
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
