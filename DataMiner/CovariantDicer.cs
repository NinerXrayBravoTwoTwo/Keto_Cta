using System.Text;
using System.Text.RegularExpressions;

namespace DataMiner;

/// <summary>
/// Represents a utility for parsing and analyzing variable names with specific patterns and extracting metadata such as
/// logarithmic, delta, and visit-related properties.
/// </summary>
/// <remarks>The <see cref="CovariantDicer"/> class validates and processes variable names that follow a specific
/// format, such as "LnD(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)". It extracts metadata about the variable, including whether it is
/// logarithmic, delta-based, or visit-related, and constructs a target string for further use.</remarks>
public class CovariantDicer
{
    public CovariantDicer(string variableName, bool isRatio = false)
    {
        VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));

        if (string.IsNullOrWhiteSpace(variableName))
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(variableName));

        // Prefix allows 'Visits[0].' or similar (word, array index, dot)
        var prefixPattern = @"(?:[a-zA-Z_][a-zA-Z0-9_]*\[\d+\]\.)?";
        var variablePattern = @"(Ln)?(D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)";
        var pattern = isRatio
            ? $"^{prefixPattern}{variablePattern}/{prefixPattern}{variablePattern}$"
            : $"^{prefixPattern}{variablePattern}$";

        var match = Regex.Match(variableName, pattern, RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new ArgumentException($"Invalid variable name: {variableName}. Expected format: '[prefix.](Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)' or 'Visits[0].(Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)/Visits[0].(Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)'");

        var varSb = new StringBuilder();

        if (match.Groups[1].Success)
            // Continue with existing logic...
        {
            varSb.Append("Ln");
            IsLogarithmic = true;
        }

        var sbRoot = new StringBuilder();
        if (match.Groups[2].Success)
        {
            IsDelta = true;
            varSb.Append('D');
            sbRoot.Append('D');
        }

        if (match.Groups[3].Success)
        {
            varSb.Append(match.Groups[3].Value);
            sbRoot.Append(match.Groups[3].Value);
        }

        // This should be invalid;  LnDTps vs. DTps, LnCac vs.Cac, etc.

        IsVisit = match.Groups[4].Success;

        // Keto_Cta.Visit
        Target = IsVisit && !string.IsNullOrEmpty(match.Groups[4].Value) ? $"Visits[{match.Groups[4].Value}].{varSb}" : varSb.ToString();
        RootAttribute = IsVisit && !string.IsNullOrEmpty(match.Groups[4].Value) ? $"Visits[{match.Groups[4].Value}].{sbRoot}" : sbRoot.ToString();
    }

    public string RootAttribute { get; set; }

    public string Target { get; set; }

    public bool IsVisit { get; set; }

    public bool IsDelta { get; set; }

    public bool IsLogarithmic { get; set; }

    public string VariableName { get; set; }

}
