namespace Import.Core.Readers;

/// <summary>
/// Template method for one-record-per-line formats.
/// Shared here: line loop, header and blank lines, line numbering, field iteration, error reporting.
/// A derived class only says how to cut a line into raw field values.
/// </summary>
public abstract class LineRecordReader<TMapping> : IRecordReader
    where TMapping : IFieldMapping
{
    protected LineRecordReader(IReadOnlyList<TMapping> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        Mappings = mappings;
    }

    protected IReadOnlyList<TMapping> Mappings { get; }

    /// <summary>Number of leading lines to skip (e.g. a CSV header).</summary>
    protected virtual int HeaderLineCount => 0;

    public IEnumerable<ImportRecord> Read(TextReader input)
    {
        // Checked here, NOT in the iterator: an iterator only runs when enumerated,
        // so the error would otherwise show up far from the faulty call.
        ArgumentNullException.ThrowIfNull(input);
        return ReadLines(input);
    }

    /// <summary>
    /// Validates the structure of the line and returns a function giving the raw text of a field.
    /// Throws FormatException if the structure is invalid.
    /// </summary>
    protected abstract Func<TMapping, string> Split(string line);

    private IEnumerable<ImportRecord> ReadLines(TextReader input)
    {
        var lineNumber = 0;
        while (input.ReadLine() is { } line)
        {
            lineNumber++;
            if (lineNumber <= HeaderLineCount || string.IsNullOrWhiteSpace(line))
                continue;

            yield return ParseLine(line, lineNumber);
        }
    }

    private ImportRecord ParseLine(string line, int lineNumber)
    {
        Func<TMapping, string> rawValueOf;
        try
        {
            rawValueOf = Split(line);
        }
        catch (FormatException ex)
        {
            throw new ImportException(lineNumber, fieldName: null, ex.Message, ex);
        }

        var record = new ImportRecord();
        foreach (var mapping in Mappings)
        {
            var raw = rawValueOf(mapping);
            try
            {
                record.Add(mapping.Name, mapping.Parser.Parse(raw));
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new ImportException(lineNumber, mapping.Name, $"invalid value '{raw}'. {ex.Message}", ex);
            }
        }

        return record;
    }
}