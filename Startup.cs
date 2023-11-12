using Microsoft.EntityFrameworkCore;
using Serilog;
using Workout.Data;
using Workout.Extensions;
using Workout.Middlewares;
using Workout.Services;

namespace Workout;

public class Startup
{
    private readonly ConfigProvider _configProvider;
    public Startup(IConfiguration configuration)
    {
        _configProvider = new ConfigProvider(configuration);

        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterServices();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_configProvider.PostgreSqlConnectionString));
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }
        else
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }

        app.UseMiddleware<ExceptionMiddleware>();

        // app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}