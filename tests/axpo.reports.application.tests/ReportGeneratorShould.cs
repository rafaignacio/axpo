using Axpo;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json;
using NSubstitute;

namespace axpo.reports.application.tests;

public class ReportGeneratorShould
{
    private IPowerService _powerService;
    private IList<PowerTrade> _example;
    private IList<ReportData> _expectedData;
    private readonly TimeProvider _defaultDate = new FakeTimeProvider(DateTimeOffset.Parse("2023-07-01T21:15Z"));

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
            (string date, int count) = (item.Date, item.Periods.Count);
            var powerTrade = PowerTrade.Create(DateTime.Parse(date), count);

            foreach (var period in item.Periods)
            {
                (int periodValue, double volume) = (period.Period, period.Volume);
                powerTrade.Periods[periodValue - 1].SetVolume(volume);
            }

            output.Add(powerTrade);
        }

        return output;
    }

    [TestCase("2023-07-01T23:15+02:00")]
    [TestCase("2023-07-02T00:15+03:00")]
    public async Task Generate_report_correctly_based_passed_date(string date)
    {
        var sut = new ReportGenerator(_powerService, new FakeTimeProvider(DateTimeOffset.Parse(date)),
            NullLogger<ReportGenerator>.Instance);

        var output = await sut.Generate();

        output.IsSuccess.Should().BeTrue();
        output.Value.Should().BeEquivalentTo(_expectedData);
    }

    [Test]
    public async Task Indicate_failure_when_power_service_does_not_return_data()
    {
        var sut = new ReportGenerator(_powerService, TimeProvider.System, NullLogger<ReportGenerator>.Instance);

        var output = await sut.Generate();

        output.IsSuccess.Should().BeFalse();
    }
}

