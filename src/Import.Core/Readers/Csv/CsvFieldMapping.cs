using Import.Core.Values;

namespace Import.Core.Readers.Csv;

public sealed class CsvFieldMapping : IFieldMapping
{
    /// <param name="name">Output field name.</param>
    /// <param name="column">1-based column index, as in the specification.</param>
    /// <param name="parser">Converts the raw column text.</param>
    public CsvFieldMapping(string name, int column, IValueParser parser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 1);
        ArgumentNullException.ThrowIfNull(parser);

        Name = name;
        Column = column;
        Parser = parser;
    }

    public string Name { get; }

    public int Column { get; }

    public IValueParser Parser { get; }
}