using Axpo;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json;
using NSubstitute;

namespace axpo.reports.application.tests;

public class ReportGeneratorShould
{
    private IPowerService _powerService;
    private IList<PowerTrade> _example;
    private IList<ReportData> _expectedData;
    private readonly TimeProvider _defaultDate = new FakeTimeProvider(DateTimeOffset.Parse("2023-07-01T23:15Z"));

    [SetUp]
    public void Setup()
    {
        _powerService = Substitute.For<IPowerService>();
        _example = GetPowerTradeExample();
        _expectedData =
            JsonConvert.DeserializeObject<IList<ReportData>>(
                File.ReadAllText("./TestData/power_trade_expected.json")) ?? throw new InvalidOperationException();

        _powerService.GetTrades(_defaultDate.GetUtcNow().UtcDateTime).Returns(_example);
        _powerService.GetTradesAsync(_defaultDate.GetUtcNow().UtcDateTime).Returns(_example);
    }

    private static IList<PowerTrade> GetPowerTradeExample()
    {
        dynamic list = JsonConvert.DeserializeObject(
            File.ReadAllText("./TestData/power_trade_example.json")) ?? throw new InvalidOperationException();
        var output = new List<PowerTrade>();

        foreach (var item in list)
        {
            var powerTrade = PowerTrade.Create(DateTime.Parse((string)item.Date), 24);

            foreach (var period in item.Periods)
            {
                var powerPeriod = powerTrade.Periods.FirstOrDefault(p => p.Period == (int)period.Period);
                powerPeriod.SetVolume((double)period.Volume);
            }

            output.Add(powerTrade);
        }

        return output;
    }

    [TestCase("2023-07-02T00:15+01:00")]
    [TestCase("2023-07-02T02:15+03:00")]
    public async Task Generate_report_correctly_based_passed_date(string date)
    {
        var sut = new ReportGenerator(_powerService, new FakeTimeProvider(DateTimeOffset.Parse(date)));

        var output = await sut.Generate();

        output.IsSuccess.Should().BeTrue();
        output.Value.Should().BeEquivalentTo(_expectedData);
    }
}

