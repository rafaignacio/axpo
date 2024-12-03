using Axpo;
using CSharpFunctionalExtensions;
using Serilog;

namespace axpo.reports.application;

public class ReportGenerator(IPowerService powerService, TimeProvider timeProvider, ILogger logger)
{
    public async Task<Result<IList<ReportData>>> Generate()
    {
        try
        {
            var date = timeProvider.GetUtcNow().UtcDateTime;
            var trades = (await powerService.GetTradesAsync(date)).ToList();

            return trades.Any()
                ? Result.Success(ProcessTradeData(date, trades))
                : Result.Failure<IList<ReportData>>(
                    $"Power Service has not returned data for the requested date of {date:O}")
;
        }
        catch (Exception e)
        {
            logger.Error(e, $"An exception occured at {DateTime.UtcNow:O} while generating report.");
            return Result.Failure<IList<ReportData>>(e.Message);
        }
    }

    private static DateTime CleanseDateTime(DateTime date) => 
        date.AddMilliseconds(-date.Millisecond)
            .AddMicroseconds(-date.Microsecond)
            .AddSeconds(-date.Second)
            .AddMinutes(-date.Minute)
            .ToUniversalTime();

    private static IList<ReportData> ProcessTradeData(DateTime date, IList<PowerTrade> data)
    {
        date = CleanseDateTime(date);
        return data.SelectMany(trade => trade.Periods)
            .GroupBy(p => p.Period)
            .Select(group => new ReportData(date.AddHours(group.Key - 1),
                Volume: group.Sum(ps => ps.Volume)))
            .OrderBy(r => r.Datetime)
            .ToList();
    }
}
