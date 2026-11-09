using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Import.Core;

/// <summary>
/// Format-agnostic representation of one imported line: ordered field name -> typed value.
/// Every reader produces it, every writer consumes it.
/// </summary>
public sealed class ImportRecord : IReadOnlyDictionary<string, object?>
{
    private readonly OrderedDictionary<string, object?> _values = new(StringComparer.Ordinal);

    public void Add(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!_values.TryAdd(name, value))
            throw new ArgumentException($"Field '{name}' is already set.", nameof(name));
    }

    public T? Get<T>(string name) => (T?)this[name];

    public object? this[string key] => _values[key];
    public IEnumerable<string> Keys => _values.Keys;
    public IEnumerable<object?> Values => _values.Values;
    public int Count => _values.Count;

    public bool ContainsKey(string key) => _values.ContainsKey(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
        => _values.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}