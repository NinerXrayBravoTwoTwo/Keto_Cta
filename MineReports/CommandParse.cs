using Keto_Cta;
using System.Text.RegularExpressions;

namespace MineReports;
// public enum Token { None, /* Other token values */ } /* From Keto_Cta */
// public enum SetName { /* Set names */ } /*From Gold Miner 

public class CommandParseResult(
    int limit,
    string[] searchTerms,
    Token dependentToken,
    Token regressionToken,
    SetName[]? setNames,
    bool isSuccess,
    string errorMessage = "")
{
    public int Limit { get; } = limit;
    public string[] SearchTerms { get; } = searchTerms ?? [];
    public Token DependentToken { get; } = dependentToken;
    public Token RegressionToken { get; } = regressionToken;
    public SetName[] SetNames { get; } = setNames ?? [];
    public bool IsSuccess { get; } = isSuccess && string.IsNullOrEmpty(errorMessage);
    public string ErrorMessage = string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage;

    public override string ToString() =>
        $"Success: {IsSuccess}, Limit: {Limit}, \"{DependentToken} vs {RegressionToken}\", " +
        $"Filters: {string.Join(',', SearchTerms)}, Sets: {string.Join(',', SetNames)}" +
        $"\nError: {ErrorMessage}";
}

public class CommandParser(string cmdName)
{
    private readonly string _cmdName = cmdName ?? throw new ArgumentNullException(nameof(cmdName));

    public CommandParseResult Parse(string cmdRequest)
    {
        var pattern = @"^todo\s*(((?:\s(,?\w+\)?\,?)\s*)+)?)?$";
        var regexPattern = pattern.Replace("todo", _cmdName.ToLower());
        var tokens = Regex.Match(cmdRequest, regexPattern, RegexOptions.IgnoreCase);

        if (!tokens.Success)
        {
            return new CommandParseResult(0, null, Token.None, Token.None, null, false, "Lol, :)  Sorry, I am not programmed to respond in that area.");
        }

        var parsedLimit = 0;
        var freeTerms = new Dictionary<string, string>();
        var listSets = new List<SetName>();
        Token depToken = Token.None;
        Token regToken = Token.None;
        var isFoundALimit = false;
        var errorMessage = string.Empty;

        for (var x = 0; x < tokens.Groups[3].Captures.Count; x++)
        {
            var param = tokens.Groups[3].Captures[x].Value;

            if (Regex.IsMatch(param, @"\,$")) errorMessage = "Invalid comma in parameter, use spaces to separate parameters.";

            if (!isFoundALimit && int.TryParse(param, out parsedLimit))
            {
                isFoundALimit = true;
            }
            else
            {
                if (regToken == Token.None && param.StartsWith("reg"))
                {
                    Enum.TryParse(param.Substring("reg".Length), true, out regToken);
                    continue;
                }
                if (depToken == Token.None && param.StartsWith("dep"))
                {
                    Enum.TryParse(param.Substring("dep".Length), true, out depToken);
                    continue;
                }
                if (Enum.TryParse(param,true, out SetName setName))
                {
                    listSets.Add(setName);
                    continue;
                }

                freeTerms.TryAdd(param.ToLower(), param);
            }
        }

        if (!isFoundALimit || parsedLimit < 1)
        {
            parsedLimit = 30;
        }

        return new CommandParseResult(parsedLimit, freeTerms.Values.ToArray(), depToken, regToken, listSets.ToArray(), true, errorMessage);
    }
}
