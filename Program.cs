using EquityHarbour.Data;
using EquityHarbour.Infrastructure;
using EquityHarbour.Models;
using EquityHarbour.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().WriteTo.Console().WriteTo.File(
    "Logs/equityharbour-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// NOTE: Default scheme is now cookies for browser-rendered MVC pages/logins.
// JWT bearer is still registered so any remaining pure-API endpoints keep working
// when called with an Authorization header instead of a cookie.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// Changed from AddControllers() -> AddControllersWithViews() to enable Razor views
builder.Services.AddControllersWithViews();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IInvestmentPlanService, InvestmentPlanService>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();
builder.Services.AddScoped<IInvestmentPayoutService, InvestmentPayoutService>();
builder.Services.AddScoped<IDepositAccountService, DepositAccountService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IGiftCodeService, GiftCodeService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddHostedService<InvestmentProcessingService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new UserViewLocationExpander());
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedAsync(services);
}
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Static files must come before routing for wwwroot (css/js/lib) to be served
app.UseStaticFiles();

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// MVC route for Razor-served pages (user panel + Admin area)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Keeps attribute-routed API controllers (e.g. [Route("api/[controller]")]) working
app.MapControllers();

try
{
    Log.Information("EquityHarbour Api is starting .......");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EquityHarbour Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}