using Axpo;
using CSharpFunctionalExtensions;

namespace axpo.reports.application;

public class ReportGenerator(IPowerService powerService, TimeProvider timeProvider)
{
    public async Task<Result<IList<ReportData>>> Generate() => Result.Failure<IList<ReportData>>("Could not generate report.");
}
