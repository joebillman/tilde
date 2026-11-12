using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Tilde.Core;

/// <summary>
/// One language's accent table: which characters have variants, and in what
/// order. Loaded from a JSON file in Languages/ (see ARCHITECTURE.md §10).
/// </summary>
public sealed class AccentMap
{
    private readonly Dictionary<char, char[]> _variants;

    private AccentMap(string code, string name, Dictionary<char, char[]> variants)
    {
        Code = code;
        Name = name;
        _variants = variants;
    }

    /// <summary>Language code, for example "es".</summary>
    public string Code { get; }

    /// <summary>Display name, for example "Español".</summary>
    public string Name { get; }

    /// <summary>Every character that has at least one variant.</summary>
    public IEnumerable<char> MappedCharacters => _variants.Keys;

    public bool IsMapped(char c) => _variants.ContainsKey(c);

    /// <summary>The variants for <paramref name="c"/> in cycle order, or an empty list.</summary>
    public IReadOnlyList<char> GetVariants(char c) =>
        _variants.TryGetValue(c, out var variants) ? variants : Array.Empty<char>();

    /// <summary>Load a language file embedded in this assembly, by language code.</summary>
    public static AccentMap Load(string languageCode)
    {
        string resourceName = $"Tilde.Core.Languages.{languageCode}.json";
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new ArgumentException($"No language file for '{languageCode}'.", nameof(languageCode));
        using var reader = new StreamReader(stream);
        return FromJson(reader.ReadToEnd());
    }

    /// <summary>Parse a language file. Throws <see cref="InvalidDataException"/> if it is not valid.</summary>
    public static AccentMap FromJson(string json)
    {
        LanguageFile? file;
        try
        {
            file = JsonSerializer.Deserialize<LanguageFile>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Language file is not valid JSON: {ex.Message}", ex);
        }

        if (file is null || string.IsNullOrWhiteSpace(file.Code) || file.Map is null)
        {
            throw new InvalidDataException("Language file needs \"code\" and \"map\".");
        }

        var variants = new Dictionary<char, char[]>();
        foreach (var (key, values) in file.Map)
        {
            char baseChar = SingleChar(key, $"key \"{key}\"");
            if (values is null || values.Length == 0)
            {
                throw new InvalidDataException($"Language '{file.Code}': key \"{key}\" has no variants.");
            }

            char[] chars = values.Select(v => SingleChar(v, $"variant \"{v}\" of \"{key}\"")).ToArray();
            variants[baseChar] = chars;

            // Capitals follow automatically: "u": ["ú","ü"] also gives "U": ["Ú","Ü"].
            // Punctuation like ? has no uppercase form, so nothing extra is added for it.
            if (char.IsLower(baseChar))
            {
                char upper = char.ToUpperInvariant(baseChar);
                variants.TryAdd(upper, chars.Select(char.ToUpperInvariant).ToArray());
            }
        }

        return new AccentMap(file.Code, file.Name ?? file.Code, variants);

        // Replacement sends exactly one Backspace, so every variant must be one
        // UTF-16 unit. A decomposed accent (e + combining mark) or an emoji
        // would leave a stray piece behind.
        char SingleChar(string? text, string what)
        {
            if (text is null || text.Length != 1 || char.IsSurrogate(text[0]) ||
                CharUnicodeInfo.GetUnicodeCategory(text[0]) == UnicodeCategory.NonSpacingMark)
            {
                throw new InvalidDataException(
                    $"Language '{file.Code}': {what} must be exactly one precomposed character.");
            }
            return text[0];
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed class LanguageFile
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, string[]?>? Map { get; set; }
    }
}
