namespace Tilde.Core;

/// <summary>
/// The list of programs Tilde stays out of (PRODUCT.md R13 and §5), stored as
/// lowercase program file names like "minecraft.exe".
/// </summary>
public static class ExclusionList
{
    public const int MaxEntries = 100;
    private const int MaxNameLength = 260;

    // Characters Windows never allows in a file name. Listed here rather than
    // asking the OS, so the tests give the same answer on any machine.
    private static readonly char[] InvalidChars = ['<', '>', ':', '"', '|', '?', '*', '\0'];

    /// <summary>
    /// Clean up one name: trim it, drop any folder path, lowercase it, and add
    /// ".exe" if missing. Returns null if nothing usable is left.
    /// </summary>
    public static string? NormalizeName(string? name)
    {
        if (name is null)
        {
            return null;
        }

        // Someone may paste a full path; only the file name matters.
        string fileName = name.Trim();
        int slash = fileName.LastIndexOfAny(['\\', '/']);
        if (slash >= 0)
        {
            fileName = fileName[(slash + 1)..];
        }
        fileName = fileName.Trim().ToLowerInvariant();

        if (fileName.Length == 0 || fileName.IndexOfAny(InvalidChars) >= 0)
        {
            return null;
        }
        if (!fileName.EndsWith(".exe", StringComparison.Ordinal))
        {
            fileName += ".exe";
        }
        return fileName.Length <= MaxNameLength ? fileName : null;
    }

    /// <summary>Normalize every name, dropping blanks, bad names, and duplicates, keeping order.</summary>
    public static List<string> Normalize(IEnumerable<string?>? names) =>
        (names ?? [])
            .Select(NormalizeName)
            .OfType<string>()
            .Distinct()
            .Take(MaxEntries)
            .ToList();

    /// <summary>Read the text box: one name per line.</summary>
    public static List<string> Parse(string? text) =>
        Normalize((text ?? "").Split(['\r', '\n']));

    /// <summary>Turn the list back into text box contents.</summary>
    public static string Format(IEnumerable<string> names) =>
        string.Join(Environment.NewLine, names);

    /// <summary>True if <paramref name="programFileName"/> (any case) is on the list.</summary>
    public static bool Contains(IReadOnlyCollection<string> list, string? programFileName)
    {
        string? name = NormalizeName(programFileName);
        return name is not null && list.Contains(name);
    }
}
