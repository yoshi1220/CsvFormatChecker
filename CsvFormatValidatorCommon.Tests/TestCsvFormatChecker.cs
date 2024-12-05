using CsvFormatCheckerCommon.Dtos;

namespace CsvFormatValidatorCommon.Tests;
public class TestCsvFormatChecker : CsvFormatCheckerCommon
{
    public TestCsvFormatChecker(Stream csvStream) : base(csvStream)
    {
    }

    protected override Task PerformSpecificChecksAsync(CsvFormatCheckResult result)
    {
        // 抽象クラスの実装をテストするため、具体的な処理は省略
        return Task.CompletedTask;
    }
}
