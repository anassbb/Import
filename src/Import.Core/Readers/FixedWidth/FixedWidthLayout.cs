namespace Import.Core.Readers.FixedWidth;

/// <summary>Describes a fixed-width file: which character range feeds which field.</summary>
public sealed class FixedWidthLayout
{
    public FixedWidthLayout(IEnumerable<FixedWidthFieldMapping> fields)
    {
        Fields = LayoutValidation.EnsureValid(fields, nameof(fields));

        for (var i = 0; i < Fields.Count; i++)
            for (var j = i + 1; j < Fields.Count; j++)
            {
                if (Fields[i].Overlaps(Fields[j]))
                    throw new ArgumentException(
                        $"Fields '{Fields[i].Name}' and '{Fields[j].Name}' overlap.", nameof(fields));
            }

        MinimumLineLength = Fields.Max(f => f.End);
    }

    public IReadOnlyList<FixedWidthFieldMapping> Fields { get; }

    public int MinimumLineLength { get; }
}