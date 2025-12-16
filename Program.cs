using System.Text;
using System.Threading.RateLimiting;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotatApp.Data;
using NotatApp.Models;
using NotatApp.Repositories;
using NotatApp.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// 1) Infrastructure / Logging
// ---------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------------------
// 2) Database
// ---------------------------
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------
// 3) Identity
// ---------------------------
builder.Services
    .AddIdentityCore<User>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        // Require email confirmation in prod; allow instant login in dev
        opt.SignIn.RequireConfirmedEmail = !builder.Environment.IsDevelopment();
        opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddLogging();

// ---------------------------
/* 4) CORS
 * Dev SPA on Vite (5173) -> API (5001)
 * Use this policy only in Development.
 */
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", p => p
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

// ---------------------------
// 5) JWT Auth
// ---------------------------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// ---------------------------
// 6) Swagger (dev only UI)
// ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------
// 7) App Services / DI
// ---------------------------
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NoteService>();

builder.Services.AddScoped<IFolderRepository, FolderRepository>();
builder.Services.AddScoped<IFolderService, FolderService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();

if (builder.Environment.IsDevelopment())
{
    // Local dev senders
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
    builder.Services.AddScoped<ISmsSender, ConsoleSmsSender>();

    // Ensure no email confirm required in dev (explicit too)
    builder.Services.Configure<IdentityOptions>(o =>
    {
        o.SignIn.RequireConfirmedEmail = false;
    });
}
else
{
    // Production: AWS SES/SNS
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonSimpleEmailService>();
    builder.Services.AddScoped<IEmailSender, SesEmailSender>();

    builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
    builder.Services.AddScoped<ISmsSender, SmsSender>();
}

// ---------------------------
// 8) Rate limiting (login)
// ---------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0; // immediately 429 if exceeded
    });
});

// ---------------------------
// 9) MVC / Errors
// ---------------------------
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Bind URL (dev)
builder.WebHost.UseUrls("http://localhost:5001");


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler(_ => { });  // enables IExceptionHandler pipeline




// ---------------------------
// 10) Migrations on startup
// ---------------------------
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    Log.Error(ex, "Migration failed.");
}

// ---------------------------
// 11) Middleware pipeline
// ---------------------------

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS (dev only)
if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}

app.UseExceptionHandler(_ => { }); // uses GlobalExceptionHandler + ProblemDetails
app.UseSerilogRequestLogging();

// Static files for SPA
// app.UseHttpsRedirection(); // enable when serving HTTPS locally
app.UseDefaultFiles();
app.UseStaticFiles();

// AuthZ
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Dev exception page (optional, on top of ProblemDetails)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Endpoints
app.MapControllers();
app.MapFallbackToFile("index.html");

// ---------------------------
// 12) Run
// ---------------------------
try
{
    Log.Information("Starting the application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start!");
}
finally
{
    Log.CloseAndFlush();
}
