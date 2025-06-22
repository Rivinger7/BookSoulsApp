using BookSoulsApp.API;
using BookSoulsApp.API.Filters;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Infrastructure.DependencyInjections;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
EnvironmentVariableLoader.LoadEnvironmentVariable();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Add a global exception filter to handle exceptions
    //options.Filters.Add(new Filters.BaseExceptionFilter(
    //    builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Filters.BaseExceptionFilter>>()));

    options.Filters.Add<BaseExceptionFilter>(); // Uncomment if you want to use the custom exception filter
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDependencyInjection();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .WithOrigins(Environment.GetEnvironmentVariable("BOOK_SOULS_CLIENT_URL") ?? throw new NotFoundCustomException("ClientUrl connect fail"))
            .WithOrigins(Environment.GetEnvironmentVariable("PAY_OS_CORE_ORIGIN") ?? throw new NotFoundCustomException("PayOs connect fail"))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration); // đọc từ appsettings.json
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.File(@"F:\Logs\AEM\log.txt"))
    .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowSpecificOrigin");

app.MapControllers();

app.Run();
