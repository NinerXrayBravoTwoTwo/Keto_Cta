using System.Text.RegularExpressions;

namespace DataMiner;

public class CreateSelector
{
    public CreateSelector(string chartTitle)
    {
        // Parse the chart title to create a selector
        // This is a placeholder for the actual implementation
        // You would need to implement the logic to parse the chart title and return a selector function

        var regSplit = Regex.Split(chartTitle, "\\s+vs.\\s*", RegexOptions.IgnoreCase);

        if (regSplit.Length == 0)
        {
            throw new ArgumentException("Chart title must not be empty.");
        }

        if (regSplit.Length < 2)
        {
            throw new ArgumentException(
                "Chart title must contain 'vs.' to separate dependent and independent variables.");
        }

        if (regSplit.Length != 2)
        {
            throw new ArgumentException("Chart title must be in the format 'Dependent vs. Independent'");
        }

        Dependant = regSplit[0].Trim();
        Independent = regSplit[1].Trim();

        if (Dependant == null || Independent == null)
        {
            throw new ArgumentException("Dependent and Independent variables cannot be null.");
        }
    }

    public string Dependant { get; set; }
    public string Independent { get; set; }
}