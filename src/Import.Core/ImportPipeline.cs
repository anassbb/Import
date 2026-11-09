using System.Text;
using Import.Core.Readers;
using Import.Core.Writers;

namespace Import.Core;

/// <summary>
/// Input format -> ImportRecord -> output format.
/// The pipeline only knows the two abstractions, so any reader can be combined with any writer.
/// Records are streamed, so large files are never fully loaded in memory.
/// </summary>
public sealed class ImportPipeline
{
    private readonly IRecordReader _reader;
    private readonly IRecordWriter _writer;

    public ImportPipeline(IRecordReader reader, IRecordWriter writer)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public void Run(TextReader input, Stream output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        _writer.Write(_reader.Read(input), output);
    }

    /// <summary>Convenience overload for small inputs (tests, demos).</summary>
    public string Run(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var input = new StringReader(content);
        using var output = new MemoryStream();
        Run(input, output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}