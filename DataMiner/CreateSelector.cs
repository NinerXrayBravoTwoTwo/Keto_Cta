using Keto_Cta;
using System.Text.RegularExpressions;

namespace DataMiner;

/// <summary>
/// Represents a selector that parses a chart title to define the relationship between  dependent and independent
/// variables, and provides a function to extract their values  from a given element.
/// </summary>
/// <remarks>The <see cref="CreateSelector"/> class is designed to interpret a chart title in the  format "X vs.
/// Y", where "X" represents the independent variable (regressor) and "Y"  represents the dependent variable (response).
/// It validates the input and ensures that  the variables are distinct. The resulting selector function can be used to
/// extract  numeric values for these variables from an object of type <see cref="Element"/>.</remarks>
public class CreateSelector
{
    //public bool IsLogMismatch => Regressor.IsLogarithmic != Dependant.IsLogarithmic; // but if regressor is logarithmic and dependant delta it is okay
    public bool IsLogMismatch => Regressor.IsLogarithmic != Dependant.IsLogarithmic
                                 && !(Regressor.IsLogarithmic && Dependant.IsDelta);
    public readonly bool IsRatio;

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
        //  Regressor contains a '/' then the regressor is a ratio expression, e.g. "LnDNcpv /LnDCac"
        if (regSplit[0].Contains('/'))
        {
            IsRatio = true;
            var ratioParts = regSplit[0].Split('/');

            if (ratioParts.Length != 2 || ratioParts.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException($"Invalid ratio expression in regressor: {regSplit[0]}");

            Numerator = new CovariantDicer(ratioParts[0].Trim(), false);
            Denominator = new CovariantDicer(ratioParts[1].Trim(), false);

            if (Numerator.RootAttribute == Denominator.RootAttribute)
                throw new ArgumentException($"Numerator and Denominator must be different: {chartTitle}");

            Regressor = new CovariantDicer($"{Numerator.RootAttribute}/{Denominator.RootAttribute}", IsRatio);
        }
        else
        {
            IsRatio = false;
            Numerator = null;
            Denominator = null;
            Regressor = new CovariantDicer(regSplit[0].Trim());
        }
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

        /* Example of how to use the selector in a regression calculation
           // This is just an example, you can remove it if not needed
           // It assumes you have a Regression class that takes a list of (double x, double y) tuples
           // and calculates the regression statistics.
           // Example usage:
           var targetElements = new List<Element>(); // Populate this with your elements
           var label = "Example Label"; // Some label for the regression
           // Calculate regression using the selector
           var regression = CalculateRegression(targetElements, label, Selector);
        
        // Calculate regression ratio if applicable
           if (IsRatio)
           {
               var regressionRatio = CalculateRegressionRatio(targetElements, label,
                   e => (Numerator.GetNestedPropertyValue(e), Denominator.GetNestedPropertyValue(e)),
                   e => Dependant.GetNestedPropertyValue(e));
           }
         *
           Regression CalculateRegression(IEnumerable<Element> targetElements, string label, Func<Element, (double x, double y)> selector)
           {
               var dataPoints = new List<(double x, double y)>();
               dataPoints.AddRange(targetElements.Select(selector));
               var regression = new RegressionPvalue(dataPoints);
               return regression;
           }

           RegressionPv CalculateRegressionRatio(IEnumerable<Element> targetElements, string label, Func<Element, (double numerator, double denominator)> xSelector, Func<Element, double> ySelector)
           {
               var dataPoints = new List<(double x, double y)>();
               dataPoints.AddRange(targetElements.Select(e =>
               {
                   var (numerator, denominator) = xSelector(e);
                   double x = denominator != 0 ? numerator / denominator : 0; // Handle division by zero
                   double y = ySelector(e);
                   return (x, y);
               }));
               var regression = new RegressionPvalue(dataPoints);
               return regression;
           }
         */
    }

    public CovariantDicer Denominator { get; set; }

    public CovariantDicer Numerator { get; set; }

    public CovariantDicer Regressor { get; set; } // X, predictor, regressor, independent variable
    public CovariantDicer Dependant { get; set; } // Y, response, dependent variable


    public Func<Element, (double x, double y)> Selector { get; init; }
    public Func<Element, (double numerator, double denominator)> XSelector => //e => (Selector(e).x, Selector(e).y);
        e => (
            Convert.ToDouble(GetNestedPropertyValue(e, Numerator?.Target ?? "")),
            Convert.ToDouble(GetNestedPropertyValue(e, Denominator?.Target ?? ""))
        );
    public Func<Element, double> YSelector => e => Selector(e).y;



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

