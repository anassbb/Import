using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Import.Core.Writers;

/// <summary>
/// Writes records as a JSON array, streaming by chunks (bounded memory).
/// Compact by default, as required by the specification.
/// </summary>
public sealed class JsonRecordWriter : IRecordWriter
{
    private const int ChunkSize = 64 * 1024;

    private readonly JsonSerializerOptions _options;
    private readonly JsonWriterOptions _writerOptions;

    public JsonRecordWriter(bool indented = false)
    {
        _options = new JsonSerializerOptions { Converters = { new NormalizedDecimalConverter() } };
        _writerOptions = new JsonWriterOptions { Indented = indented };
    }

    public void Write(IEnumerable<ImportRecord> records, Stream output)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(output);

        // JSON is produced in a memory buffer and copied to the output by chunks of ~64 KB:
        // memory stays bounded for big files, and a failure before the first chunk
        // leaves the output empty instead of a truncated "[{...".
        var buffer = new ArrayBufferWriter<byte>(ChunkSize);
        using var writer = new Utf8JsonWriter(buffer, _writerOptions);

        writer.WriteStartArray();
        foreach (var record in records)
        {
            WriteRecord(writer, record);

            writer.Flush(); // to the in-memory buffer only
            if (buffer.WrittenCount >= ChunkSize)
                CopyTo(buffer, output);
        }
        writer.WriteEndArray();

        writer.Flush();
        CopyTo(buffer, output);
        output.Flush();
    }

    private void WriteRecord(Utf8JsonWriter writer, ImportRecord record)
    {
        writer.WriteStartObject();
        foreach (var (name, value) in record)
        {
            writer.WritePropertyName(name);
            if (value is null)
                writer.WriteNullValue();
            else
                JsonSerializer.Serialize(writer, value, value.GetType(), _options);
        }
        writer.WriteEndObject();
    }

    private static void CopyTo(ArrayBufferWriter<byte> buffer, Stream output)
    {
        output.Write(buffer.WrittenSpan);
        buffer.ResetWrittenCount();
    }

    /// <summary>
    /// System.Text.Json keeps the decimal scale (1.250m -> "1.250"); the expected output is 1.25.
    /// Dividing by 1.000...0 (28 zeros, max scale) strips the trailing zeros without changing the value.
    /// </summary>
    private sealed class NormalizedDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDecimal();

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value / 1.0000000000000000000000000000m);
    }
}