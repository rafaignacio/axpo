using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

static IServiceProvider ConfigServices()
{
    var serviceCollection = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

    serviceCollection.AddSingleton<App>();
    serviceCollection.AddSingleton(Log.Logger);
    serviceCollection.AddSingleton<IConfiguration>(configuration);

    return serviceCollection.BuildServiceProvider();
}

var app = ConfigServices().GetRequiredService<App>();
await app.RunAsync(new CancellationTokenSource().Token);

public class App(ILogger logger, IConfiguration configuration)
{
    public ValueTask RunAsync(CancellationToken cancellationToken)
    {
        var intervalInMinutes = configuration.GetValue<string>("intervalInMinutes");
        logger.Information(
            "Initiating application with a time interval of {Minutes} minutes",
            intervalInMinutes
        );

        return ValueTask.CompletedTask;
    }
}
