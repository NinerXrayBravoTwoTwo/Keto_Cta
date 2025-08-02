using System;
using System.Data;
using System.Text.RegularExpressions;

namespace DataMiner
{
    public partial class CreateSelector
    {
        #region Compile properties

        public static (Token token, string numerator, string denominator) Compile(string regressorOrDependent)
        {
            var attribute = AttributeCaseNormalize(regressorOrDependent);

            // Check for ratio and ln(ratio) and rip all the parens out of the numerator and denominator
            var ratioMatch = Regex.Match(regressorOrDependent, @"(Ln\()?\(?([A-Za-z\d]+)/([A-Za-z\d]+)\)?", RegexOptions.IgnoreCase);
            if (ratioMatch.Success)
            {
                var numerator = Compile(ratioMatch.Groups[2].Value);
                var denominator = Compile(ratioMatch.Groups[3].Value);

                if (numerator.token == Token.Ratio)
                    throw new SyntaxErrorException($"No, I am not going to do recursive trees of ratios of ratios, <sigh/>,  'ratio vs ratio' is okay however ... call me <smile/> Re: {regressorOrDependent})");

                return (ratioMatch.Groups[1].Success // The secret to our success is the first group
                        ? Token.LnRatio : Token.Ratio, numerator.numerator, denominator.numerator);
            }
            
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
            var element = Regex.Match(attribute, @"^(Ln)*(DTps|DCac|DNcpv|DTcpv|DPav|DQangio)$", RegexOptions.Compiled);
            if (element.Success)
            {
                // prefix element[x] to attribute without ending \d
                return (
                    Token.ElementAttribute,
                    $"{element.Groups[1]}{element.Groups[2]}",
                    string.Empty
                );
            }

            // else throw syntax error you dumb ...
            throw new SyntaxErrorException($"Not a valid data point: {attribute}");
        }

        private static readonly Dictionary<string, string> AttributeDictionary = new Dictionary<string, string>(
            "DTps|DCac|DNcpv|DTcpv|DPav|DQangio|Tps|Cac|Ncpv|Tcpv|Pav|Qangio"
                .Split('|')
                .SelectMany(att => new[] { new KeyValuePair<string, string>(att.ToLower(), att),
                                  new KeyValuePair<string, string>("ln" + att.ToLower(), "Ln" + att) })
        );

        public static string AttributeCaseNormalize(string attribute)
        {
            var noSpaceAttribute = Regex.Replace(attribute, @"\s+", string.Empty);

            if (noSpaceAttribute.Contains('/'))
                return noSpaceAttribute;
            
            var match = Regex.Match(noSpaceAttribute, @"(^[a-zA-Z)]+)(\d)$", RegexOptions.Compiled);

            if (match.Success)
            {
                var suffix = match.Groups[2].Value;
                var key = match.Groups[1].Value.ToLower();
                return AttributeDictionary[key] + suffix;
            }
            else
            {
                var key = noSpaceAttribute.ToLower();
                if (AttributeDictionary.TryGetValue(key, out var normalized))
                    return normalized;
            }

            throw new SyntaxErrorException($"Valid attribute not found for:{noSpaceAttribute}");
        }

        #endregion

        private static string RemoveParens(string value)
        {
            // Remove parentheses from the value
            return Regex.Replace(value, @"[\(\)]", string.Empty);
        }
    }
}

