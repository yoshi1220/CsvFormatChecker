namespace CsvFormatCheckerCommon.Dtos;

public class CsvFormatCheckResult
{
    public List<FormatCheckErrorMessage> FormatCheckErrorMessages { get; }
    public bool HasErrors => FormatCheckErrorMessages.Any();
    /// <summary>
    /// 
    /// </summary>
    public CsvFormatCheckResult()
    {
        FormatCheckErrorMessages = new List<FormatCheckErrorMessage>();
    }
    public void AddError(int? rowNumber, string message)
    {
        FormatCheckErrorMessages.Add(new FormatCheckErrorMessage
        {
            RowNumber = rowNumber,
            ErrorMessage = message
        });
    }
}
