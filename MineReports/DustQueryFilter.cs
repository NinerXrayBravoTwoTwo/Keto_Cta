namespace MineReports;

/// <summary>
/// Filters dust rows the way the console <c>dust</c> command does, with two differences:
/// set names match exactly (a search for Zeta does not match Eta or BetaUZeta), and an
/// optional maximum p-value drops weaker fits. The limit keeps the most significant rows.
/// </summary>
public static class DustQueryFilter
{
    public const int DefaultLimit = 500;

    public static IReadOnlyList<T> Apply<T>(
        IEnumerable<T> rows,
        string? search,
        int limit,
        double? maxP,
        IReadOnlyCollection<string>? sets,
        Func<T, string> title,
        Func<T, string> setName,
        Func<T, double?> pValue)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(setName);
        ArgumentNullException.ThrowIfNull(pValue);

        var terms = string.IsNullOrWhiteSpace(search)
            ? []
            : search.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var setLookup = sets is null
            ? null
            : new HashSet<string>(sets, StringComparer.OrdinalIgnoreCase);

        var filtered = rows.Where(row =>
        {
            if (setLookup is not null && !setLookup.Contains(setName(row)))
                return false;

            if (terms.Length > 0 && !terms.Any(term =>
                    title(row).Contains(term, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (maxP is { } cutoff)
            {
                var p = pValue(row);
                if (p is null || double.IsNaN(p.Value) || double.IsInfinity(p.Value) || p.Value > cutoff)
                    return false;
            }

            return true;
        });

        var take = limit <= 0 ? DefaultLimit : limit;
        return filtered
            .OrderBy(row => Rank(pValue(row)))
            .ThenBy(row => pValue(row) ?? double.MaxValue)
            .Take(take)
            .ToList();
    }

    private static int Rank(double? p) => p is null || double.IsNaN(p.Value) ? 1 : 0;
}
