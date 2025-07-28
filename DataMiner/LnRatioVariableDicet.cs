using System.Text.RegularExpressions;

namespace DataMiner;

public class LnRatioVariableDicer : BaseVariableDicer
{
    public SimpleVariableDicer Numerator { get; }
    public SimpleVariableDicer Denominator { get; }

    public LnRatioVariableDicer(string variableName)
    {
        VariableName = variableName;

        // Normalize input to trim extra whitespace
        var normalizedInput = Regex.Replace(variableName.Trim(), @"\s+", " ");
        System.Diagnostics.Debug.WriteLine($"Normalized Input: {normalizedInput}");

        // Support format: Ln([prefix.](D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)/[prefix.](D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?))
        var prefixPattern = @"(?:[a-zA-Z_][a-zA-Z0-9_]*\[\d+\]\.)?";
        var variablePattern = @"(D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)";
        var pattern = $@"^Ln\({prefixPattern}{variablePattern}\s*/\s*{prefixPattern}{variablePattern}\)$";

        var match = Regex.Match(normalizedInput, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            System.Diagnostics.Debug.WriteLine($"Regex failed for groups: {string.Join(", ", match.Groups.Cast<Group>().Select(g => $"[{g.Index}]: {g.Value}"))}");
            throw new ArgumentException($"Invalid ratio variable name: {variableName}. Expected format: 'Ln([prefix.](D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)/[prefix.](D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?))'");
        }

        System.Diagnostics.Debug.WriteLine($"Matched groups: {string.Join(", ", match.Groups.Cast<Group>().Select(g => $"[{g.Index}]: {g.Value}"))}");

        // Extract numerator and denominator parts
        var numD = match.Groups[1].Success ? "D" : "";
        var numAttr = match.Groups[2].Success ? match.Groups[2].Value : throw new ArgumentException($"Numerator attribute missing: {variableName}");
        var numIndex = match.Groups[3].Value;
        var denD = match.Groups[4].Success ? "D" : "";
        var denAttr = match.Groups[5].Success ? match.Groups[5].Value : throw new ArgumentException($"Denominator attribute missing: {variableName}");
        var denIndex = match.Groups[6].Value;

        // Check if numerator or denominator starts with Ln
        if (numAttr.StartsWith("Ln", StringComparison.OrdinalIgnoreCase) || denAttr.StartsWith("Ln", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Numerator and denominator in 'Ln(.../...)' format cannot start with 'Ln': {variableName}");

        var numeratorName = $"{numD}{numAttr}{numIndex}";
        var denominatorName = $"{denD}{denAttr}{denIndex}";

        System.Diagnostics.Debug.WriteLine($"NumeratorName: {numeratorName}, DenominatorName: {denominatorName}");

        if (string.IsNullOrEmpty(numeratorName) || string.IsNullOrEmpty(denominatorName))
            throw new ArgumentException($"Invalid numerator or denominator name: numerator='{numeratorName}', denominator='{denominatorName}' for input: {variableName}");

        try
        {
            Numerator = new SimpleVariableDicer(numeratorName);
            Denominator = new SimpleVariableDicer(denominatorName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SimpleVariableDicer failed for numerator: {numeratorName}, denominator: {denominatorName}");
            throw new ArgumentException($"Failed to create SimpleVariableDicer for numerator '{numeratorName}' or denominator '{denominatorName}': {ex.Message}", ex);
        }

        if (Numerator.Target == Denominator.Target)
            throw new ArgumentException($"Numerator and Denominator must be different: {variableName}");

        Target = $"{Numerator.Target}/{Denominator.Target}";
        RootAttribute = Target;
        IsLogarithmic = true;
        IsDelta = Numerator.IsDelta || Denominator.IsDelta;
        IsVisit = Numerator.IsVisit || Denominator.IsVisit;
    }
}