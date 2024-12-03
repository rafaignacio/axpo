using axpo.reports.application;
using Axpo;
using axpo.console;
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
    serviceCollection.AddSingleton(TimeProvider.System);
    serviceCollection.AddSingleton<IConfiguration>(configuration);
    serviceCollection.AddTransient<IPowerService, PowerService>();
    serviceCollection.AddTransient<ReportGenerator>();

    return serviceCollection.BuildServiceProvider();
}

var app = ConfigServices().GetRequiredService<App>();
await app.RunAsync(new CancellationTokenSource().Token);

