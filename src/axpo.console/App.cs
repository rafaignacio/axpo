using System.Text;
using axpo.reports.application;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Polly;
using Serilog;

namespace axpo.console;

public class App(ReportGenerator reportGenerator, ILogger logger, IConfiguration configuration)
{
    public async ValueTask RunAsync(CancellationToken cancellationToken)
    {
        var intervalInMinutes = int.Parse(configuration.GetValue<string>("intervalInMinutes")!);
        logger.Information(
            "Initiating application with a time interval of {Minutes} minutes",
            intervalInMinutes
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var reportData = await Policy<Result<IList<ReportData>>>
                    .HandleResult(result => result.IsFailure)
                    .WaitAndRetryAsync(12, sleepDurationProvider: _ => TimeSpan.FromSeconds(5), onRetry: (c, t) =>
                    {
                        var msg = c.Result.IsFailure ? c.Result.Error : c.Exception.Message;
                        logger.Warning(new ApplicationException(msg), $"Failed to generate report. Retrying in {t:g}.");
                    })
                    .ExecuteAsync(reportGenerator.Generate);

                if (reportData.IsFailure)
                    throw new ApplicationException(reportData.Error);

                CreateReportFile(reportData.Value);
            }
            catch (Exception e)
            {
                logger.Error(e, "Couldn't generate report data." );
            }

            logger.Information("Next execution at {timeStamp}.", TimeProvider.System.GetLocalNow().AddMinutes(intervalInMinutes));
            await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), cancellationToken);
        }

        logger.Information("Application was cancelled.");
    }

    private void CreateReportFile(IList<ReportData> data)
    {
        var date = TimeProvider.System.GetLocalNow();
        var filePath = configuration.GetValue<string>("filePath");
        Directory.CreateDirectory(filePath!);
        var fileName = $"{filePath}PowerPosition_{date.AddDays(1):yyyyMMdd}_{date.DateTime:yyyyMMddHHmm}.csv";
        var content = new StringBuilder();
        content.AppendLine("Datetime;Volume");

        foreach (var reportData in data)
        {
            content.AppendLine($"{reportData.Datetime:O};{reportData.Volume:F}");
        }

        File.WriteAllText(fileName, content.ToString());
        logger.Information("Report {@fileName} created.", fileName);
    }
}
