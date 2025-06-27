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

        Selector = new Func<Element, (double x, double y)>(item =>
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

    private object? GetNestedPropertyValue(object obj, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        foreach (var property in properties)
        {
            if (obj == null) return null;
            var propInfo = obj.GetType().GetProperty(property);
            if (propInfo == null) return null;
            obj = propInfo.GetValue(obj);
        }
        return obj;
    }
}

// Example usage:
// var selector = new CreateSelector("LnDcac vs. Ncpv");
// var result = selector.Selector(new Element("1", new List<Visit> { new Visit(), new Visit() }));
// Console.WriteLine($"X: {result.x}, Y: {result.y}");
//         for (var x = 0; x < deltaAttributes.Length; x++)
//             for (var y = 0; y < visitAttributes.Length; y++)
//                 if (x != y)
//                     charts.Add($"{deltaAttributes[x]} vs. {visitAttributes[y]}");
//         return charts.ToArray();
//     }


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

        Target = IsVisit && !string.IsNullOrEmpty(match.Groups[4].Value) ? $"Visits[{match.Groups[4].Value}].{varSb}" : varSb.ToString();

    }

    public string Target { get; set; }

    public int NumberedIndex { get; set; }

    public bool IsVisit { get; set; }

    public bool IsDelta { get; set; }

    public bool IsLogarithmic { get; set; }

    public string VariableName { get; set; }

}