using System.Text.RegularExpressions;

namespace DataMiner;

public class RatioVariableDicer : BaseVariableDicer
{
    public SimpleVariableDicer Numerator { get; }
    public SimpleVariableDicer Denominator { get; }

    public RatioVariableDicer(string variableName)
    {
        ValidateVariableName(variableName);
        VariableName = variableName;

        // Allow spaces around the '/' separator
        var prefixPattern = @"(?:[a-zA-Z_][a-zA-Z0-9_]*\[\d+\]\.)?";
        var variablePattern = @"(Ln)?(D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)";
        var pattern = $"^{prefixPattern}{variablePattern}\\s*/\\s*{prefixPattern}{variablePattern}$";

        var match = Regex.Match(variableName, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new ArgumentException($"Invalid ratio variable name: {variableName}. Expected format: '[prefix.](Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)/[prefix.](Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)'");

        // Extract numerator and denominator parts
        var numLn = match.Groups[1].Success ? "Ln" : "";
        var numD = match.Groups[2].Success ? "D" : "";
        var numAttr = match.Groups[3].Value;
        var numIndex = match.Groups[4].Value;
        var numeratorName = $"{numLn}{numD}{numAttr}{numIndex}";

        var denLn = match.Groups[5].Success ? "Ln" : "";
        var denD = match.Groups[6].Success ? "D" : "";
        var denAttr = match.Groups[7].Value;
        var denIndex = match.Groups[8].Value;
        var denominatorName = $"{denLn}{denD}{denAttr}{denIndex}";

        Numerator = new SimpleVariableDicer(numeratorName);
        Denominator = new SimpleVariableDicer(denominatorName);

        if (Numerator.Target == Denominator.Target)
            throw new ArgumentException($"Numerator and Denominator must be different: {variableName}");

        Target = $"{Numerator.Target}/{Denominator.Target}";
        RootAttribute = Target;
        IsLogarithmic = Numerator.IsLogarithmic || Denominator.IsLogarithmic;
        IsDelta = Numerator.IsDelta || Denominator.IsDelta;
        IsVisit = Numerator.IsVisit || Denominator.IsVisit;
    }
}