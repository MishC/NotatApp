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
        opt.SignIn.RequireConfirmedEmail = false;
        opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


//Frontend CORS header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
        "https://noteappsolutions.com", "https://www.noteappsolutions.com", "http://localhost:5173") // Allow frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials (optional)
    });
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
        ClockSkew = TimeSpan.Zero
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



app.UseExceptionHandler(o => { });
app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

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





