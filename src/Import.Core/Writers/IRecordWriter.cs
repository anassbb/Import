namespace Import.Core.Writers;

/// <summary>
/// Writes records to an output.
/// Extension point: a new output format (XML, CSV, database...) = a new implementation.
/// </summary>
public interface IRecordWriter
{
    void Write(IEnumerable<ImportRecord> records, Stream output);
}