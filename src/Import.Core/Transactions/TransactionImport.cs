using Import.Core.Readers;
using Import.Core.Readers.Csv;
using Import.Core.Readers.FixedWidth;
using Import.Core.Registry;
using Import.Core.Writers;
using static Import.Core.Transactions.TransactionFields;
using static Import.Core.Values.ValueParsers;

namespace Import.Core.Transactions;

/// <summary>
/// Configuration of the kata: the only place that knows the transaction layouts.
/// The engine itself (readers, writers, pipeline) is generic and reusable for other data.
/// </summary>
public static class TransactionImport
{
    public const string CsvFormat = "csv";
    public const string FixedWidthFormat = "fixed";
    public const string JsonFormat = "json";

    public static CsvLayout CsvLayout { get; } = new(
    [
        new CsvFieldMapping(OriginalSourceId, column: 1, Text),
        new CsvFieldMapping(TransactionDate,  column: 2, Date("yyyy-MM-dd")),
        new CsvFieldMapping(Value,            column: 3, DecimalNumber),
        new CsvFieldMapping(Rate,             column: 4, DecimalNumber),
    ]);

    public static FixedWidthLayout FixedWidthLayout { get; } = new(
    [
        new FixedWidthFieldMapping(OriginalSourceId, start: 1,  end: 4,  Text),
        new FixedWidthFieldMapping(TransactionDate,  start: 5,  end: 12, Date("yyyyMMdd")),
        new FixedWidthFieldMapping(Value,            start: 13, end: 20, ImpliedDecimal(2)),
        new FixedWidthFieldMapping(Rate,             start: 21, end: 25, ImpliedDecimal(3)),
    ]);

    public static FormatRegistry<IRecordReader> CreateReaderRegistry() => new FormatRegistry<IRecordReader>()
        .Register(CsvFormat, () => new CsvRecordReader(CsvLayout), ".csv")
        .Register(FixedWidthFormat, () => new FixedWidthRecordReader(FixedWidthLayout), ".txt", ".dat", ".fwf");

    public static FormatRegistry<IRecordWriter> CreateWriterRegistry() => new FormatRegistry<IRecordWriter>()
        .Register(JsonFormat, () => new JsonRecordWriter(), ".json");

    public static ImportPipeline CreatePipeline(string inputFormat)
        => new(CreateReaderRegistry().Create(inputFormat), new JsonRecordWriter());

    public static ImportPipeline CreateCsvPipeline() => CreatePipeline(CsvFormat);

    public static ImportPipeline CreateFixedWidthPipeline() => CreatePipeline(FixedWidthFormat);
}