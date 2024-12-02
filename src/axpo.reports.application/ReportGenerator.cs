using Axpo;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace axpo.reports.application;

public class ReportGenerator(IPowerService powerService, TimeProvider timeProvider, ILogger<ReportGenerator> logger)
{
    public async Task<Result<IList<ReportData>>> Generate()
    {
        try
        {
            var trades = await powerService.GetTradesAsync(timeProvider.GetUtcNow().UtcDateTime);

            return Result.Success(ProcessTradeData(trades.ToList()));
        }
        catch (Exception e)
        {
            logger.LogError($"An exception occured at {DateTime.UtcNow:O} while generating report.", e);
            return Result.Failure<IList<ReportData>>(e.Message);
        }
    }

    private static IList<ReportData> ProcessTradeData(IList<PowerTrade> data) => throw new NotImplementedException();
}
