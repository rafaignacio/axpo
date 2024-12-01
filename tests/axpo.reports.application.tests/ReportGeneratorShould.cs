namespace axpo.reports.application.tests;

public class ReportGeneratorShould
{
    private ReportGenerator? _sut;

    [SetUp]
    public void Setup() {
        _sut = new(null, new FakeTimeProvider())
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}

