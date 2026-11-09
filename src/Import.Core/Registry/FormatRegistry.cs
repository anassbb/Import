namespace Import.Core.Registry;

/// <summary>
/// Name/extension -> factory catalogue, used for readers and writers alike.
/// Adding a format = one Register call at startup: no switch to modify (open/closed principle).
/// </summary>
public sealed class FormatRegistry<T> where T : class
{
    private readonly Dictionary<string, Func<T>> _factories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _formatByExtension = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> Formats => _factories.Keys;

    public FormatRegistry<T> Register(string format, Func<T> factory, params string[] extensions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(extensions);

        if (_factories.ContainsKey(format))
            throw new ArgumentException($"Format '{format}' is already registered.", nameof(format));

        var normalizedExtensions = extensions.Select(NormalizeExtension).ToList();
        foreach (var extension in normalizedExtensions)
        {
            if (_formatByExtension.TryGetValue(extension, out var owner))
                throw new ArgumentException($"Extension '{extension}' is already handled by '{owner}'.", nameof(extensions));
        }

        // Checks first, then writes: a rejected call leaves the registry unchanged.
        _factories.Add(format, factory);
        foreach (var extension in normalizedExtensions)
            _formatByExtension.Add(extension, format);

        return this; // fluent: .Register(...).Register(...)
    }

    public T Create(string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        return _factories.TryGetValue(format, out var factory)
            ? factory()
            : throw new NotSupportedException(
                $"Unknown format '{format}'. Available: {string.Join(", ", _factories.Keys)}.");
    }

    public string DetectFormat(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extension = Path.GetExtension(filePath);
        return _formatByExtension.TryGetValue(extension, out var format)
            ? format
            : throw new NotSupportedException(
                $"No format registered for extension '{extension}'. Use an explicit format.");
    }

    public T CreateForFile(string filePath) => Create(DetectFormat(filePath));

    private static string NormalizeExtension(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        return extension.StartsWith('.') ? extension : "." + extension;
    }
}