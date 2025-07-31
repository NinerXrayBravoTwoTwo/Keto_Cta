using System.Data;
using System.Text.RegularExpressions;

namespace DataMiner
{
    public partial class CreateSelectorTwo
    {
        #region Compile properties

        public static (Token token, string numerator, string denominator) Compile(string regressorOrDependent)
        {
            var attribute = AttributeCaseNormalize(regressorOrDependent);

            // if visit attribute
            var visit = Regex.Match(attribute, @"^(Ln)*(Tps|Cac|Ncpv|Tcpv|Pav|Qangio)(\d+)$");
            if (visit.Success)
            {
                // prefix visit[x] to attribute without ending \d
                return (
                    Token.VisitAttribute,
                    $"Visits[{visit.Groups[3]}].{visit.Groups[1]}{visit.Groups[2]}",
                    string.Empty
                );
            }

            //if element attribute
            var element = Regex.Match(attribute, @"^(Ln)*(DTps|DCac|DNcpv|DTcpv|DPav|DQangio)$");
            if (element.Success)
            {
                // prefix element[x] to attribute without ending \d
                return (
                    Token.ElementAttribute,
                    $"{element.Groups[1]}{element.Groups[2]}",
                    string.Empty
                );
            }

            // if ratio

            // if ln(ratio)

            // else throw syntax error you dumb ...
            throw new SyntaxErrorException($"Not a valid data point: {attribute}");
        }

        private static Dictionary<string, string> AttributeDictionary = new Dictionary<string, string>(
            "DTps|DCac|DNcpv|DTcpv|DPav|DQangio|Tps|Cac|Ncpv|Tcpv|Pav|Qangio"
                .Split('|')
                .SelectMany(att => new[] { new KeyValuePair<string, string>(att.ToLower(), att),
                                  new KeyValuePair<string, string>("ln" + att.ToLower(), "Ln" + att) })
        );

        public static string AttributeCaseNormalize(string attribute)
        {
            var match = Regex.Match(attribute, @"(^[a-zA-Z)]+)(\d)$");

            //var result = string.Empty;
            if (match.Success)
            {
                var suffix = match.Groups[2].Value;
                var key = match.Groups[1].Value.ToLower();
                return AttributeDictionary[key] + suffix;
            }
            else
            {
                var key = attribute.ToLower();
                if (AttributeDictionary.TryGetValue(key, out var normalized))
                    return normalized;
            }

            throw new SyntaxErrorException($"Valid attribute not found for:{attribute}");
        }

        #endregion
    }
}

