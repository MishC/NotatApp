using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Repositories;
using NotatApp.Services;
using NotatApp.Models;
using Serilog.Events;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using Amazon.SimpleEmail;
//using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

//DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Microsoft ASP.NET Identity handler
// ASP.NET Core Identity (uses ApplicationDbContext)


builder.Services
    .AddIdentityCore<User>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;

        // Dev
        opt.SignIn.RequireConfirmedEmail = !builder.Environment.IsDevelopment();

        opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    builder.Services.AddLogging();


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", p =>
        p.WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});


//Check JWT Token validity
//AddJwtBearer is a middleware which verify and validate tokens
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //using of [Authorize]
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true,

    };
});

builder.Services.AddAuthorization();

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); //for Swagger only
builder.Services.AddSwaggerGen();
//End of Swagger config

builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NoteService>();

builder.Services.AddScoped<IFolderRepository, FolderRepository>();
builder.Services.AddScoped<IFolderService, FolderService>();

//Tokens
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();


if (builder.Environment.IsDevelopment())
{
    // LOCALLY
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
        builder.Services.AddScoped<ISmsSender, ConsoleSmsSender>();         // ★ add this

    builder.Services.Configure<IdentityOptions>(o =>
    {
        // No email confirmation requirement in local dev
        o.SignIn.RequireConfirmedEmail = false;
    });
}
else  //PRODUCTION ONLY
{
   
    // EMAIL:::::::  EC2 / PROD: SES -Emails
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonSimpleEmailService>();
    builder.Services.AddScoped<IEmailSender, SesEmailSender>();

    // SMS::::: external API SMS -//EC2 SNS

    builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

    builder.Services.AddScoped<ISmsSender, SmsSender>();
}


 builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1); // 1 minute window
        opt.PermitLimit = 5;                  // max 5 login tries/minute
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                   // 429
    });
});


builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.WebHost.UseUrls("http://localhost:5001");



var app = builder.Build();

// APPLY MIGRATIONS ON STARTUP
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    Log.Error(ex, " Migration failed. ");

}


// Enable CORS middleware
app.UseCors("AllowReactApp");

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
//DEV ONLY
if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}

app.UseExceptionHandler(o => { });
app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}


app.MapControllers();

app.MapFallbackToFile("index.html");

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





