namespace Import.Core.Readers.Csv;

/// <summary>
/// Simple CSV reader (no quoted fields: not needed by the specification).
/// Supporting quotes would only change Split.
/// </summary>
public sealed class CsvRecordReader : LineRecordReader<CsvFieldMapping>
{
    private readonly CsvLayout _layout;

    public CsvRecordReader(CsvLayout layout)
        : base((layout ?? throw new ArgumentNullException(nameof(layout))).Fields)
    {
        _layout = layout;
    }

    protected override int HeaderLineCount => _layout.HasHeader ? 1 : 0;

    protected override Func<CsvFieldMapping, string> Split(string line)
    {
        var columns = line.Split(_layout.Separator);

        // Strict count: an extra column (e.g. "50,50.89" with a decimal comma) would silently shift values.
        if (columns.Length != _layout.ColumnCount)
            throw new FormatException($"Expected {_layout.ColumnCount} columns but found {columns.Length}.");

        return field => columns[field.Column - 1];
    }
}