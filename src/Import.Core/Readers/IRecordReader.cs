namespace Import.Core.Readers;

/// <summary>
/// Reads an input and yields format-agnostic records.
/// Extension point: a new input format = a new implementation.
/// </summary>
public interface IRecordReader
{
    /// <summary>Lazily reads the records. Throws ImportException on invalid data.</summary>
    IEnumerable<ImportRecord> Read(TextReader input);
}