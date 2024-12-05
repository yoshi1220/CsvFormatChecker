namespace CsvFormatValidatorCommon.Tests;

public class CsvFormatCheckerCommonTests
{
    /// <summary>
    /// UTF-8 BOM�t���̗L����CSV���e�X�g���܂��B
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidUtf8Bom_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_utf8_bom.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "�G���[���������Ȃ��͂��ł�");
    }

    /// <summary>
    /// UTF-8 BOM�Ȃ��̗L����CSV���e�X�g���܂��B
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidUtf8NoBom_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_utf8_nobom.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "�G���[���������Ȃ��͂��ł�");
    }

    /// <summary>
    /// Shift-JIS�̗L����CSV���e�X�g���܂��B
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_ValidSjis_ReturnsNoError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "valid_sjis.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.False(result.HasErrors, "�G���[���������Ȃ��͂��ł�");
    }

    /// <summary>
    /// �s���ȃG���R�[�h��CSV���e�X�g���A�G���[���������邱�Ƃ��m�F���܂��B
    /// </summary>
    [Fact]
    public async Task CheckFormatAsync_InvalidEncoding_ReturnsError()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "invalid_encoding.csv");
        using var stream = File.OpenRead(path);
        var checker = new TestCsvFormatChecker(stream);

        var result = await checker.CheckFormatAsync();

        Assert.True(result.HasErrors, "�G���[����������͂��ł�");
        Assert.Contains("�����R�[�h���s��", result.FormatCheckErrorMessages[0].ErrorMessage);
    }
}
