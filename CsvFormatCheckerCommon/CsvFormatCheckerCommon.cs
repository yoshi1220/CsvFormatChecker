using CsvFormatCheckerCommon.Dtos;
using System.Text;
using Ude;

namespace CsvFormatValidatorCommon;

/// <summary>
/// CSVファイルの基本的なフォーマットチェックを行う抽象クラスです。
/// 継承先で特定のフォーマットに依存したチェックを実装することを想定しています。
/// </summary>
public abstract class CsvFormatCheckerCommon : IDisposable
{
    /// <summary>
    /// 最大レコード数の制限値。
    /// </summary>
    protected static readonly int _maxRecords = 40_000;

    /// <summary>
    /// チェック対象となるCSVファイルのストリーム。
    /// </summary>
    protected readonly Stream _csvStream;

    /// <summary>
    /// <see cref="CsvFormatCheckerCommon"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="csvStream">チェック対象となるCSVファイルのストリーム。</param>
    /// <exception cref="ArgumentNullException">csvStreamがnullの場合に発生します。</exception>
    protected CsvFormatCheckerCommon(Stream csvStream)
    {
        _csvStream = csvStream ?? throw new ArgumentNullException(nameof(csvStream));
    }

    /// <summary>
    /// CSVファイルのフォーマットチェックを実行します。
    /// 基本チェック（空ファイル、文字コード、レコード数）後、派生クラスで定義する個別チェックを行います。
    /// </summary>
    /// <returns>チェック結果を保持する <see cref="CsvFormatCheckResult"/> を返します。</returns>
    public async Task<CsvFormatCheckResult> CheckFormatAsync()
    {
        var result = new CsvFormatCheckResult();

        try
        {
            var basicCheckResult = await PerformBasicChecksAsync();
            if (basicCheckResult.HasErrors)
            {
                return basicCheckResult;
            }

            await PerformSpecificChecksAsync(result);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CSVフォーマットチェック中にエラーが発生: {ex}");
            result.AddError(null, "予期せぬエラーが発生しました。");
            return result;
        }
    }

    /// <summary>
    /// 派生クラスで実行される特定のフォーマット要件に基づいたチェックを行います。
    /// </summary>
    /// <param name="result">チェック結果を格納する <see cref="CsvFormatCheckResult"/>。</param>
    /// <returns>非同期操作を表すタスク。</returns>
    protected abstract Task PerformSpecificChecksAsync(CsvFormatCheckResult result);

    /// <summary>
    /// CSVファイルに対する基本的なフォーマットチェックを実行します。
    /// 空ファイル、文字コード、レコード数などの基本項目を確認します。
    /// </summary>
    /// <returns>チェック結果を保持する <see cref="CsvFormatCheckResult"/> を返します。</returns>
    protected virtual async Task<CsvFormatCheckResult> PerformBasicChecksAsync()
    {
        var result = new CsvFormatCheckResult();

        // 空ファイルチェック
        if (await IsEmptyFileAsync())
        {
            result.AddError(null, "CSVファイルが空です。1行以上のデータが必要です。");
            return result;
        }

        // 文字コードチェック
        var (isValidEncoding, encodingErrorMessage) = await IsValidEncodingAsync();
        if (!isValidEncoding)
        {
            result.AddError(null, encodingErrorMessage);
            return result;
        }

        // レコード数チェック
        if (!await IsValidRecordCountAsync())
        {
            result.AddError(null, $"レコード数が上限を超えています。{_maxRecords}件以内にしてください。");
            return result;
        }

        return result;
    }

    /// <summary>
    /// CSVファイルが空であるかどうかを判定します。
    /// </summary>
    /// <returns>
    /// 空の場合はtrue、それ以外の場合はfalse。
    /// </returns>
    private async Task<bool> IsEmptyFileAsync()
    {
        if (_csvStream.Length == 0)
            return true;

        _csvStream.Position = 0;
        using var reader = new StreamReader(_csvStream, Encoding.UTF8, true);
        var firstLine = await reader.ReadLineAsync();
        _csvStream.Position = 0;
        return string.IsNullOrEmpty(firstLine);
    }

    /// <summary>
    /// CSVファイルの文字コードがUTF-8(BOMあり/なし)またはShift-JISであるかを判定します。
    /// udeライブラリによるエンコーディング推定を用います。
    /// </summary>
    /// <returns>
    /// isValidがtrueの場合、encodingErrorMessageは空文字列。  
    /// isValidがfalseの場合、encodingErrorMessageにエラー内容が設定されます。
    /// </returns>
    private async Task<(bool isValid, string errorMessage)> IsValidEncodingAsync()
    {
        // UTF-8 BOMチェック
        _csvStream.Position = 0;
        var bom = new byte[3];
        await _csvStream.ReadAsync(bom, 0, 3);
        _csvStream.Position = 0;
        if (bom.Length >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            return (true, string.Empty);
        }

        // udeによるエンコーディング判定
        var encoding = await DetectEncodingAsync();
        if (encoding == null)
        {
            return (false, "文字コードが不正です。UTF-8（BOMあり/なし）またはShift-JISで保存してください。");
        }

        var name = encoding.WebName.ToLowerInvariant();
        return (name == "utf-8" || name == "shift_jis")
            ? (true, string.Empty)
            : (false, "文字コードが不正です。UTF-8（BOMあり/なし）またはShift-JISで保存してください。");
    }

    /// <summary>
    /// udeライブラリを使用して、与えられたストリームのエンコーディングを推定します。
    /// </summary>
    /// <returns>
    /// 推定された <see cref="Encoding"/> を返します。
    /// 推定できなかった場合はnullを返します。
    /// </returns>
    private async Task<Encoding?> DetectEncodingAsync()
    {
        _csvStream.Position = 0;
        using var memory = new MemoryStream();
        await _csvStream.CopyToAsync(memory);
        memory.Position = 0;

        var detector = new CharsetDetector();
        var buffer = new byte[4096];
        int read;
        while ((read = memory.Read(buffer, 0, buffer.Length)) > 0 && !detector.IsDone())
        {
            detector.Feed(buffer, 0, read);
        }
        detector.DataEnd();
        _csvStream.Position = 0;

        if (!string.IsNullOrEmpty(detector.Charset))
        {
            try
            {
                return Encoding.GetEncoding(detector.Charset);
            }
            catch
            {
                // GetEncodingに失敗した場合はnullを返す。
            }
        }

        return null;
    }

    /// <summary>
    /// CSVファイルの行数が上限値(_maxRecords)以内かどうかを判定します。
    /// </summary>
    /// <returns>
    /// 行数が上限値以内であればtrue、超過していればfalseを返します。
    /// </returns>
    private async Task<bool> IsValidRecordCountAsync()
    {
        _csvStream.Position = 0;
        using var reader = new StreamReader(_csvStream, Encoding.UTF8, true);

        int lineCount = 0;
        while (await reader.ReadLineAsync() != null)
        {
            if (++lineCount > _maxRecords)
            {
                _csvStream.Position = 0;
                return false;
            }
        }

        _csvStream.Position = 0;
        return true;
    }

    /// <summary>
    /// アンマネージリソースの破棄を行います。
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 既に破棄されているかどうかを示すフラグです。
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 派生クラスでアンマネージリソースおよびマネージリソースを破棄するために使用するメソッドです。
    /// </summary>
    /// <param name="disposing">マネージリソースも破棄する場合はtrue。</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _csvStream?.Dispose();
        }
        _disposed = true;
    }
}
