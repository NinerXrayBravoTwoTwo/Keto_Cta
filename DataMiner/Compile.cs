using System.Data;
using System.Text.RegularExpressions;

namespace DataMiner;

public static class Compile
{
    private static readonly Dictionary<string, string> AttributeDictionary = new Dictionary<string, string>(
        "DTps|DCac|DNcpv|DTcpv|DPav|DQangio|Tps|Cac|Ncpv|Tcpv|Pav|Qangio"
            .Split('|')
            .SelectMany(att => new[] {
                new KeyValuePair<string, string>(att.ToLower(), att),
                new KeyValuePair<string, string>("ln" + att.ToLower(), "Ln" + att) })
    );

    public static (Token token, string numerator, string denominator) Build(string regressorOrDependent)
    {

        #region design
        //A- single attribute, Cac0, LnDCac1, DTps2, etc.
        //    o Group[4].success. Contents equal group[1].
        //    o num is in G4
        //    o den is null or 1, 
        //    o token.VisitAttribute if ends in integer element attribute if no number suffix
        //    o All other groups false

        //B- ratio, e.g. Cac0/Cac1, DTps2/DTps3, etc.
        //    o G1, G4, G5 success
        //    o G1 contains num
        //    o G5 contains den with prefix /
        //    o Token .Ratio

        //C- ln(num/den), e.g. Ln(Cac0/Cac1), Ln(DTps2/DTps3), etc.
        //    o G1, G2, G3 success
        //    o G2 contains num
        //    o G3 contains den with prefix /
        //    o Token .LnRatio

        //D- ln(var0), single variable, not a ratio, replace with LnVar0
        //    o G1, G2 success
        //    o G2 contains var0, 
        //    o G3 is empty
        //    o G1 starts with ln(
        //    o num , den is empty
        //    o Token .VisitAttribute if ends in integer element attribute if no number suffix
        #endregion

        // if ratio compile tokens, num and den, separately (recurse)
        //                             Normal Ratio i.e. num / den       .............  Ln( ratio) i.e. Ln( num / den)
        var regSpaceRemove = new Regex(@"(^/|\s*)");
        var regDep = regSpaceRemove.Replace(regressorOrDependent, string.Empty);

        var tokenize = @"^(ln\(([A-Z\d]+)(\s*/\s*[A-Z\d]+)?\)|([A-Z\d]+)(\s*/\s*[A-Z\d]+)?)$";
        var tokens = Regex.Match(regDep, tokenize, RegexOptions.IgnoreCase);
        if (!tokens.Success)
        {
            var msg = $"Bad attribute syntax '{regDep}' is invalid.";
            System.Diagnostics.Debug.WriteLine(msg);
            throw new SyntaxErrorException(msg);
        }

        #region ln(var3) -> lnvar3 transfom
        { // ln(Var) -> LnVar ...
            if (tokens.Groups[2].Success && !tokens.Groups[3].Success)
            {
                var lnToLnVar = Regex.Match(tokens.Groups[2].Value, @"^([A-Z]+)(\d)?$", RegexOptions.IgnoreCase);
                if (!lnToLnVar.Success)
                    throw new ArgumentException(
                        $"That's unexpected ... do not use Ln(Var) use LnVar instead. Ln(A/B) is allowed.");

                bool isVisit = lnToLnVar.Groups[2].Success; // trailing digit :)e : string.Empty); // + 0 or 1 Visit array index

                string numerator =
                    isVisit
                        ? $"Visits[{lnToLnVar.Groups[2]}].Ln{lnToLnVar.Groups[1].Value}"
                        : "Ln" + lnToLnVar.Groups[1].Value;

                string denominator = string.Empty;

                return (isVisit
                        ? Token.VisitAttribute
                        : Token.ElementAttribute,
                    numerator, denominator);
            }
        }
        #endregion

        #region ratio and ln(ratio)
        {
            var isRatio = tokens.Groups[4].Success && tokens.Groups[5].Success;
            var isLnRatio = tokens.Groups[2].Success && tokens.Groups[3].Success;


            if (isRatio || isLnRatio)
            {
                var numerator = isRatio
                    ? Compile.Build(tokens.Groups[4].Value)
                    : Compile.Build(tokens.Groups[2].Value);

                var denG3 = regSpaceRemove.Replace(tokens.Groups[3].Value, string.Empty);
                var denG5 = regSpaceRemove.Replace(tokens.Groups[5].Value, string.Empty);

                var denominator =
                    isRatio
                        ? Compile.Build(denG5)
                        : Compile.Build(denG3);

                if (numerator.token == Token.Ratio || denominator.token == Token.Ratio)
                    throw new SyntaxErrorException(
                        $"No, I am not going to do recursive trees of ratios of ratios, <sigh/>, 'ratio vs ratio' is okay however ... call me <Grin/> Re: {regDep})");

                // The secret to our success is the first group
                return (isRatio
                        ? Token.Ratio
                        : Token.LnRatio,
                    numerator.numerator, denominator.numerator);
            }
        }
        #endregion

        #region single attribute, map as visit[index] or element
        //A- single attribute, Cac0, LnDCac1, DTps2, etc. (r00t)
        // This is not the same as Ratio above, it
        if (!tokens.Groups[4].Success)
            throw new SyntaxErrorException($"Not a valid element or visit attribute: {regDep}");

        {
            // Group[4] is the numerator, no denominator
            var numerator = AttributeCaseNormalize(tokens.Groups[4].Value);

            // Check for visit attribute
            var visitElement = Regex.Match(numerator, @"^\s*((Ln|LnD)?[A-Z)]+)(\d)?\s*$", RegexOptions.IgnoreCase);
            if (visitElement.Success)
            {
                // if there is a G3 this is a Visit attribute else element attribute
                bool isVisit = visitElement.Groups[3].Success && visitElement.Groups[1].Success;
                bool isElement = !isVisit;
                if (!(isVisit || isElement))
                    throw new SyntaxErrorException("The attribute '{attribute} parse error. Must be in Visit or Element.");

                string comp =
                    isVisit
                        ? $"Visits[{visitElement.Groups[3]}].{visitElement.Groups[1].Value}"
                        : visitElement.Groups[1].Value;

                return (
                    isVisit ? Token.VisitAttribute : Token.ElementAttribute,
                    comp,
                    string.Empty
                );
            }
        }
        #endregion

        throw new SyntaxErrorException($"Unexpected regression compile failure; Possibly not a valid element or visit attribute: {regDep}");
    }

    public static string AttributeCaseNormalize(string attribute)
    {

        if (attribute.Contains('/'))
            throw new SyntaxErrorException($"Valid attribute not found for: '{attribute}'");

        var match = Regex.Match(attribute, @"^(Ln)?([A-Z)]+)(\d)?$", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var suffix = match.Groups[3].Value;
            var key = match.Groups[1] + match.Groups[2].Value;
            return AttributeDictionary[key.ToLower()] + suffix;
        }
        else
        {
            var key = attribute.ToLower();
            if (AttributeDictionary.TryGetValue(key, out var normalized))
                return normalized;
        }

        throw new SyntaxErrorException($"Valid attribute not found for: '{attribute}'");
    }
}