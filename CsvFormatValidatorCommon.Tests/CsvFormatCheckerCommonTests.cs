namespace CsvFormatValidatorCommon.Tests;

public class CsvFormatCheckerCommonTests
{
    /// <summary>
    /// UTF-8 BOM付きの有効なCSVをテストします。
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidUtf8Bom_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_utf8_bom.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "エラーが発生しないはずです");
    }

    /// <summary>
    /// UTF-8 BOMなしの有効なCSVをテストします。
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidUtf8NoBom_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_utf8_nobom.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "エラーが発生しないはずです");
    }

    /// <summary>
    /// Shift-JISの有効なCSVをテストします。
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidSjis_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_sjis.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "エラーが発生しないはずです");
    }

    /// <summary>
    /// 不正なエンコードのCSVをテストし、エラーが発生することを確認します。
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_InvalidEncoding_ReturnsError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "invalid_encoding.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.True(result.HasErrors, "エラーが発生するはずです");
        Assert.Contains("文字コードが不正", result.FormatCheckErrorMessages[0].ErrorMessage);
    }
}
