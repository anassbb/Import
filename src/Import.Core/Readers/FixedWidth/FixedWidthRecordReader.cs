namespace Import.Core.Readers.FixedWidth;

public sealed class FixedWidthRecordReader : LineRecordReader<FixedWidthFieldMapping>
{
    private readonly FixedWidthLayout _layout;

    public FixedWidthRecordReader(FixedWidthLayout layout)
        : base((layout ?? throw new ArgumentNullException(nameof(layout))).Fields)
    {
        _layout = layout;
    }

    protected override Func<FixedWidthFieldMapping, string> Split(string line)
    {
        if (line.Length < _layout.MinimumLineLength)
            throw new FormatException(
                $"Expected a line of at least {_layout.MinimumLineLength} characters but found {line.Length}.");

        // 1-based inclusive positions -> 0-based index for Substring.
        return field => line.Substring(field.Start - 1, field.Length);
    }
}