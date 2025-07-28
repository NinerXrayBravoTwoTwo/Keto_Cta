using System.Text;
using System.Text.RegularExpressions;

namespace DataMiner;

public class SimpleVariableDicer : BaseVariableDicer
{
    public SimpleVariableDicer(string variableName)
    {
        ValidateVariableName(variableName);
        VariableName = variableName;

        // Normalize input to trim extra whitespace
        var normalizedInput = Regex.Replace(variableName.Trim(), @"\s+", " ");
        System.Diagnostics.Debug.WriteLine($"Normalized Input: {normalizedInput}");

        // Support format: [prefix.](Ln)D(DTps|Tps|DCac|Cac|DNcpv|Ncpv|DTcpv|Tcpv|DPav|Pav)(\d?)
        var prefixPattern = @"(?:[a-zA-Z_][a-zA-Z0-9_]*\[\d+\]\.)?";
        var variablePattern = @"(Ln)?(D)?(DTps|Tps|DCac|Cac|DNcpv|Ncpv|DTcpv|Tcpv|DPav|Pav)(\d?)";
        var pattern = $@"^{prefixPattern}{variablePattern}$";

        var match = Regex.Match(normalizedInput, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            System.Diagnostics.Debug.WriteLine($"Regex failed for groups: {string.Join(", ", match.Groups.Cast<Group>().Select(g => $"[{g.Index}]: {g.Value}"))}");
            throw new ArgumentException($"Invalid variable name: {variableName}. Expected format: '[prefix.](Ln)D(DTps|Tps|DCac|Cac|DNcpv|Ncpv|DTcpv|Tcpv|DPav|Pav)(\\d?)'");
        }

        System.Diagnostics.Debug.WriteLine($"Matched groups: {string.Join(", ", match.Groups.Cast<Group>().Select(g => $"[{g.Index}]: {g.Value}"))}");

        var varSb = new StringBuilder();
        var sbRoot = new StringBuilder();

        bool isDelta = match.Groups[2].Success;
        IsLogarithmic = match.Groups[1].Success;

        if (isDelta)
        {
            varSb.Append('D');
            sbRoot.Append('D');
            IsDelta = true;
        }

        if (match.Groups[3].Success)
        {
            varSb.Append(match.Groups[3].Value);
            sbRoot.Append(match.Groups[3].Value);
        }

        IsVisit = match.Groups[4].Success && !string.IsNullOrEmpty(match.Groups[4].Value);
        var visitIndex = IsVisit ? match.Groups[4].Value : "0";

        if (isDelta && !IsVisit)
        {
            Target = varSb.ToString();
            RootAttribute = sbRoot.ToString();
        }
        else
        {
            Target = $"Visits[{visitIndex}].{varSb}";
            RootAttribute = $"Visits[{visitIndex}].{sbRoot}";
        }

        System.Diagnostics.Debug.WriteLine($"Target: {Target}, RootAttribute: {RootAttribute}");
    }
}