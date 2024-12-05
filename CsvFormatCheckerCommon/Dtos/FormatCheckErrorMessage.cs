namespace CsvFormatCheckerCommon.Dtos;

/// <summary>
/// フォーマットチェックエラーメッセージクラス
/// 各1件ごとのエラーメッセージを格納する
/// </summary>
public class FormatCheckErrorMessage
{
    /// <summary>
    /// エラーが発生した行番号
    /// 全体に関わるエラーの場合はnull
    /// </summary>
    public int? RowNumber { get; set; }
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public required string ErrorMessage { get; set; }
}
