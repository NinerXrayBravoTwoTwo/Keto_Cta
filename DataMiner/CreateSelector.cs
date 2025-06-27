using System.Text;
using Keto_Cta;
using System.Text.RegularExpressions;

namespace DataMiner;

public class CreateSelector
{
    public CreateSelector(string chartTitle)
    {
        // Parse the chart title to create a selector
        // This is a placeholder for the actual implementation
        // You would need to implement the logic to parse the chart title and return a selector function

        var regSplit = Regex.Split(chartTitle, @"\s+vs.\s*", RegexOptions.IgnoreCase);

        switch (regSplit.Length)
        {
            case 0:
                throw new ArgumentException("Chart title must not be empty.");
            case < 2:
                throw new ArgumentException(
                    "Chart title must contain 'vs.' to separate dependent and independent variables.");
        }

        Regressor = new CovariantDicer(regSplit[0].Trim());
        Dependant = new CovariantDicer(regSplit[1].Trim());

            if (Regressor == null || Dependant == null)
                throw new ArgumentException("Dependent and Independent variables cannot be null.");

            if (Regressor.Target.Equals(Dependant.Target, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Dependent and Independent variables must be different.");

            Selector = new Func<Element?, (double x, double y)>(item =>
            {
                var xValue = GetNestedPropertyValue(item, Regressor.Target);
                var yValue = GetNestedPropertyValue(item, Dependant.Target);
                if (xValue == null || yValue == null)
                {
                    throw new ArgumentException($"Properties '{Regressor.Target}' or '{Dependant.Target}' not found in Element.");
                }
                return (Convert.ToDouble(xValue), Convert.ToDouble(yValue));
            });
    }

    public CovariantDicer Regressor { get; set; } // X, predictor, regressor, independent variable
    public CovariantDicer Dependant { get; set; } // Y, response,  dependent variable

    public bool IsLogMismatch => Regressor.IsLogarithmic != Dependant.IsLogarithmic;

    public Func<Element, (double x, double y)>? Selector { get; set; }

    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        var regex = new Regex(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?");
        var properties = propertyPath.Split('.');

        foreach (var property in properties)
        {
            if (obj == null) return null;

            var match = regex.Match(property);
            if (!match.Success) return null;

            var propName = match.Groups[1].Value;
            var propInfo = obj.GetType().GetProperty(propName);
            if (propInfo == null) return null;

            obj = propInfo.GetValue(obj);

            // Handle indexer if present
            if (match.Groups[2].Success && obj is System.Collections.IList list)
            {
                int index = int.Parse(match.Groups[3].Value);
                if (index < 0 || index >= list.Count) return null;
                obj = list[index];
            }
        }
        return obj;
    }
}


/// <summary>
/// Represents a utility for parsing and analyzing variable names with specific patterns and extracting metadata such as
/// logarithmic, delta, and visit-related properties.
/// </summary>
/// <remarks>The <see cref="CovariantDicer"/> class validates and processes variable names that follow a specific
/// format, such as "LnD(Tps|Cac|Ncpv|Tcpv|Pav)(\d?)". It extracts metadata about the variable, including whether it is
/// logarithmic, delta-based, or visit-related, and constructs a target string for further use.</remarks>
public class CovariantDicer
{
    // Fix for CS0200: Property or indexer 'Group.Success' cannot be assigned to -- it is read only.
    // The issue is that `Group.Success` is a read-only property and cannot be assigned directly.
    // Instead, we should use separate logic to set the corresponding boolean fields.

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

        IsVisit = match.Groups[4].Success;
        // {Keto_Cta.Visit
        Target = IsVisit && !string.IsNullOrEmpty(match.Groups[4].Value) ? $"Visits[{match.Groups[4].Value}].{varSb}" : varSb.ToString();

    }

    public string Target { get; set; }

   //public int NumberedIndex { get; set; }

    public bool IsVisit { get; set; }

    public bool IsDelta { get; set; }

    public bool IsLogarithmic { get; set; }

    public string VariableName { get; set; }

}