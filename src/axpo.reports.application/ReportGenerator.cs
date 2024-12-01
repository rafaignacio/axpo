using Axpo;
using CSharpFunctionalExtensions;

namespace axpo.reports.application;

public class ReportGenerator(IPowerService powerService, TimeProvider timeProvider)
{
    public static Result Generate() => Result.Failure("Could not generate report.");
}
