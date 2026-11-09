using Import.Core;
using Import.Core.Transactions;

// Usage: Import.Cli <input-file> [--format <csv|fixed>] [--output <json>]
// Without --format, the input format is detected from the file extension.

if (args.Length == 0 || args.Length % 2 == 0)
{
    PrintUsage();
    return 1;
}

var inputPath = args[0];
string? inputFormat = null;
var outputFormat = TransactionImport.JsonFormat;

for (var i = 1; i < args.Length; i += 2)
{
    switch (args[i])
    {
        case "--format": inputFormat = args[i + 1]; break;
        case "--output": outputFormat = args[i + 1]; break;
        default:
            PrintUsage();
            return 1;
    }
}

var readers = TransactionImport.CreateReaderRegistry();
var writers = TransactionImport.CreateWriterRegistry();

try
{
    var reader = inputFormat is null ? readers.CreateForFile(inputPath) : readers.Create(inputFormat);
    var pipeline = new ImportPipeline(reader, writers.Create(outputFormat));

    using var input = File.OpenText(inputPath);
    using var output = Console.OpenStandardOutput();
    pipeline.Run(input, output);
    Console.WriteLine();
    return 0;
}
catch (Exception ex) when (ex is ImportException or NotSupportedException or IOException)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}

void PrintUsage()
{
    Console.Error.WriteLine("Usage: Import.Cli <input-file> [--format <format>] [--output <format>]");
    Console.Error.WriteLine($"  Input formats : {string.Join(", ", TransactionImport.CreateReaderRegistry().Formats)}");
    Console.Error.WriteLine($"  Output formats: {string.Join(", ", TransactionImport.CreateWriterRegistry().Formats)}");
}