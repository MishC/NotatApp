using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Repositories;
using NotatApp.Services;
using NotatApp.Models;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

//DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
//Microsoft ASP.NET Identity handler
// ASP.NET Core Identity (uses ApplicationDbContext)
builder.Services
    .AddIdentityCore<User>(opt => {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        opt.SignIn.RequireConfirmedEmail = false; // flip to true in prod
        opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//Check Token validity

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(o =>
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
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// external API
builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
builder.Services.AddSingleton<ISmsSender, TwilioSmsSender>();



builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.WebHost.UseUrls("http://localhost:5001");



var app = builder.Build();

// APPLY MIGRATIONS ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();   // ← vytvorí notes.db a aplikuje migrácie
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





