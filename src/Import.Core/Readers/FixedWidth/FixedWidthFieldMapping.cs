using Import.Core.Values;

namespace Import.Core.Readers.FixedWidth;

public sealed class FixedWidthFieldMapping : IFieldMapping
{
    /// <param name="name">Output field name.</param>
    /// <param name="start">1-based first character, inclusive (as in the specification: "13-20").</param>
    /// <param name="end">1-based last character, inclusive.</param>
    /// <param name="parser">Converts the raw slice.</param>
    public FixedWidthFieldMapping(string name, int start, int end, IValueParser parser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(start, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);
        ArgumentNullException.ThrowIfNull(parser);

        Name = name;
        Start = start;
        End = end;
        Parser = parser;
    }

    public string Name { get; }

    public int Start { get; }

    public int End { get; }

    public int Length => End - Start + 1;

    public IValueParser Parser { get; }

    public bool Overlaps(FixedWidthFieldMapping other) => Start <= other.End && other.Start <= End;
}