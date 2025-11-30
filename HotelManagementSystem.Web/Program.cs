using HotelManagementSystem.Web.Components;
using HotelManagementSystem.Web.Data;
using HotelManagementSystem.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Fix for PostgreSQL DateTime Kind=Local issue
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers(); // Required for AccountController

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationCore();
// builder.Services.AddProtectedBrowserStorage(); // It is added by default in .NET 8+ Blazor Server usually, but let's be safe if needed or check if it's there.
// Actually, AddInteractiveServerComponents adds it.

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var provider = builder.Configuration["DatabaseProvider"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<HotelDbContext>(options =>
{
    switch (provider?.ToLower())
    {
        case "mysql":
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
        case "postgres":
        case "postgresql":
            options.UseNpgsql(connectionString);
            break;
        case "sqlite":
        default:
            options.UseSqlite(connectionString);
            break;
    }
});

builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<GuestService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ConfigurationService>();
builder.Services.AddScoped<BrandingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Ensure database is created and migrated in all environments
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var config = services.GetRequiredService<IConfiguration>();
        var dbProvider = config["DatabaseProvider"];
        logger.LogInformation($"[MIGRATION] Starting database migration. Provider: {dbProvider}");

        var context = services.GetRequiredService<HotelDbContext>();

        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation(
                $"[MIGRATION] Pending migrations found: {string.Join(", ", context.Database.GetPendingMigrations())}");
            context.Database.Migrate();
            logger.LogInformation("[MIGRATION] Database migrated successfully.");
        }
        else
        {
            logger.LogInformation("[MIGRATION] No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[MIGRATION] An error occurred while migrating the database.");
    }
}

var supportedCultures = new[] { "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapControllers(); // Add this to map the AccountController
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
