﻿using DataMiner;
using System.Text;
using System.Text.RegularExpressions;

public class SimpleVariableDicer : BaseVariableDicer
{
    public SimpleVariableDicer(string variableName)
    {
        ValidateVariableName(variableName);
        VariableName = variableName;

        var prefixPattern = @"(?:[a-zA-Z_][a-zA-Z0-9_]*\[\d+\]\.)?";
        var variablePattern = @"(Ln)?(D)?(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)";
        var pattern = $"^{prefixPattern}{variablePattern}$";

        var match = Regex.Match(variableName, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new ArgumentException($"Invalid variable name: {variableName}. Expected format: '[prefix.](Ln)D(Tps|Cac|Ncpv|Tcpv|Pav)(\\d?)'");

        var varSb = new StringBuilder();
        var sbRoot = new StringBuilder();

        bool isDelta = match.Groups[2].Success;
        if (match.Groups[1].Success)
        {
            varSb.Append("Ln");
            IsLogarithmic = true;
            sbRoot.Append(isDelta ? "LnD" : "Ln");
        }

        if (isDelta)
        {
            varSb.Append('D');
            if (!IsLogarithmic)
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

        if (isDelta && !IsVisit || IsLogarithmic && varSb.ToString().StartsWith("LnD") && !IsVisit)
        {
            Target = varSb.ToString();
            RootAttribute = sbRoot.ToString();
        }
        else
        {
            Target = $"Visits[{visitIndex}].{varSb}";
            RootAttribute = $"Visits[{visitIndex}].{sbRoot}";
        }
    }
}