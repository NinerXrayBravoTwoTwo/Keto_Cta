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

        if (Regressor.RootAttribute.Equals(Dependant.RootAttribute))
        {
            // WTF? This is a regression of the same attribute.
            throw new ArgumentException($"Dependent and Independent variables must be different. {chartTitle}");
        }

        if (Regressor == null || Dependant == null)
            throw new ArgumentException("Dependent and Independent variables cannot be null.");

        if (Regressor.Target.Equals(Dependant.Target, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Dependent and Independent variables must be different.");

        Selector = item =>
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Element cannot be null.");
            }
            var xValue = GetNestedPropertyValue(item, Regressor.Target);
            var yValue = GetNestedPropertyValue(item, Dependant.Target);
            if (xValue == null || yValue == null)
            {
                throw new ArgumentException($"Properties '{Regressor.Target}' or '{Dependant.Target}' not found in Element.");
            }
            return (Convert.ToDouble(xValue), Convert.ToDouble(yValue));
        };
    }

    public CovariantDicer Regressor { get; set; } // X, predictor, regressor, independent variable
    public CovariantDicer Dependant { get; set; } // Y, response, dependent variable

    //public bool IsLogMismatch => Regressor.IsLogarithmic != Dependant.IsLogarithmic; // but if regressor is logarithmic and dependant delta it is okay
    public bool IsLogMismatch => Regressor.IsLogarithmic != Dependant.IsLogarithmic
                                  && !(Regressor.IsLogarithmic && Dependant.IsDelta);

    public Func<Element, (double x, double y)> Selector { get; init; }

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

