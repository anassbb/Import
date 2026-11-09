namespace Import.Core;

/// <summary>Raised when an input line cannot be imported. Carries the line number and, when known, the field.</summary>
public sealed class ImportException : Exception
{
    public ImportException(int lineNumber, string? fieldName, string message, Exception? innerException = null)
        : base(BuildMessage(lineNumber, fieldName, message), innerException)
    {
        LineNumber = lineNumber;
        FieldName = fieldName;
    }

    public int LineNumber { get; }

    public string? FieldName { get; }

    private static string BuildMessage(int lineNumber, string? fieldName, string message)
        => fieldName is null
            ? $"Line {lineNumber}: {message}"
            : $"Line {lineNumber}, field '{fieldName}': {message}";
}