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
    public CovariantDicer(string variableName)
    {
        VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));

        if (string.IsNullOrWhiteSpace(variableName))
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(variableName));

        // Validate the variable name format
        var match = Regex.Match(variableName, @"^(Ln)?(D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)$", RegexOptions.IgnoreCase);

        if (!match.Success)
            throw new ArgumentException($"Invalid variable name: {variableName}. Expected format: 'LnD(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)'.");

        var varSb = new StringBuilder();

        if (match.Groups[1].Success)
        {
            varSb.Append("Ln");
            IsLogarithmic = true;
        }

        if (match.Groups[2].Success)
        {
            IsDelta = true;
            varSb.Append("D");
        }

        if (match.Groups[3].Success)
        {
            varSb.Append(match.Groups[3].Value);
        }

        // This should be invalid;  LnDTps vs. DTps, LnCac vs.Cac, etc.

        IsVisit = match.Groups[4].Success;
        // Keto_Cta.Visit
        Target = IsVisit && !string.IsNullOrEmpty(match.Groups[4].Value) ? $"Visits[{match.Groups[4].Value}].{varSb}" : varSb.ToString();

    }

    public string Target { get; set; }

    public bool IsVisit { get; set; }

    public bool IsDelta { get; set; }

    public bool IsLogarithmic { get; set; }

    public string VariableName { get; set; }

}
