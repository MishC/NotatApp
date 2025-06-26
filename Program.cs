using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Repositories;
using NotatApp.Services;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//Frontend CORS header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://51.20.51.192:5173",
        "https://51.20.51.192:5173", "https://localhost:5173", 
        "https://noteappsolutions.com", "https://www.noteappsolutions.com") // Allow frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials (optional)
    });
});


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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NoteService>();

builder.Services.AddScoped<IFolderRepository, FolderRepository>();
builder.Services.AddScoped<IFolderService, FolderService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.WebHost.UseUrls("http://localhost:5001");



var app = builder.Build();

// Enable CORS middleware
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(o => { });  
app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();


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



app.Run();


