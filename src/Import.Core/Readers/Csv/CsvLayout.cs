namespace Import.Core.Readers.Csv;

/// <summary>Describes a CSV file: which column feeds which field, separator, header, column count.</summary>
public sealed class CsvLayout
{
    /// <param name="fields">Field mappings, in output order.</param>
    /// <param name="separator">Column separator.</param>
    /// <param name="hasHeader">True if the first line is a header to skip.</param>
    /// <param name="columnCount">
    /// Exact number of columns expected on each line. Defaults to the highest mapped column;
    /// set it when the file has extra columns that are not imported.
    /// </param>
    public CsvLayout(IEnumerable<CsvFieldMapping> fields, char separator = ',', bool hasHeader = false, int? columnCount = null)
    {
        Fields = LayoutValidation.EnsureValid(fields, nameof(fields));

        var duplicateColumn = Fields.GroupBy(f => f.Column).FirstOrDefault(g => g.Count() > 1);
        if (duplicateColumn is not null)
            throw new ArgumentException($"Column {duplicateColumn.Key} is mapped more than once.", nameof(fields));

        var highestColumn = Fields.Max(f => f.Column);
        if (columnCount < highestColumn)
            throw new ArgumentOutOfRangeException(nameof(columnCount),
                $"Column count {columnCount} is lower than the highest mapped column {highestColumn}.");

        Separator = separator;
        HasHeader = hasHeader;
        ColumnCount = columnCount ?? highestColumn;
    }

    public IReadOnlyList<CsvFieldMapping> Fields { get; }

    public char Separator { get; }

    public bool HasHeader { get; }

    public int ColumnCount { get; }
}