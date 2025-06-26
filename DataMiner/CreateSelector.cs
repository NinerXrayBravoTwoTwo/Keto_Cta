using System.Text.RegularExpressions;
using Keto_Cta;

namespace DataMiner;

public class CreateSelector
{
    public CreateSelector(string chartTitle)
    {
        // Parse the chart title to create a selector
        // This is a placeholder for the actual implementation
        // You would need to implement the logic to parse the chart title and return a selector function

        var regSplit = Regex.Split(chartTitle, "\\s+vs.\\s*", RegexOptions.IgnoreCase);

        switch (regSplit.Length)
        {
            case 0:
                throw new ArgumentException("Chart title must not be empty.");
            case < 2:
                throw new ArgumentException(
                    "Chart title must contain 'vs.' to separate dependent and independent variables.");
        }

        Dependant = regSplit[0].Trim();
        Independent = regSplit[1].Trim();

        if (Dependant == null || Independent == null)
        {
            throw new ArgumentException("Dependent and Independent variables cannot be null.");
        }

        Selector = new Func<Element, (double x, double y)>(item =>
        {
            var xValue = item.GetType().GetProperty(Dependant)?.GetValue(item);
            var yValue = item.GetType().GetProperty(Independent)?.GetValue(item);
            if (xValue == null || yValue == null)
            {
                throw new ArgumentException($"Properties '{Dependant}' or '{Independent}' not found in Element.");
            }
            return (Convert.ToDouble(xValue), Convert.ToDouble(yValue));
        });
    }

    public string Dependant { get; set; }
    public string Independent { get; set; }
    public Func<Element,(double x, double y)>? Selector { get; set; }
}