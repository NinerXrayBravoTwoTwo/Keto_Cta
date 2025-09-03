using Keto_Cta;
using System.Text.RegularExpressions;
using static DataMiner.MazeTwistyPassages;

public enum Token
{
    None = 0,
    Visit = 1,
    Element = 2,
    Ratio = 3,
    LnRatio = 4,
    GeoMean = 5,
    RankA = 6,
    RankD = 7
}

namespace DataMiner
{
    public class CreateSelector
    {
        public CreateSelector(string dependentVsRegressor)
        {
            // delete any whitespace
            var regressionString = dependentVsRegressor.Trim();

            var vsPattern = @"^(ln\([A-Z\d]+(\s*/\s*[A-Z\d]+)?\)|[A-Z\d]+(\s*/\s*[A-Z\d]+)?)\s+(vs\.?)\s+(ln\([A-Z\d]+(\s*/\s*[A-Z\d]+)?\)|[A-Z\d]+(\s*/\s*[A-Z\d]+)?)\s*$";
            var match = Regex.Match(regressionString, vsPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new ArgumentException($"Invalid regression string format: {regressionString}");

            var dependent = match.Groups[1].Value;
            var regressor = match.Groups[5].Value;
            Title = $"{dependent} vs {regressor}";

            DependentCompile = Compile.Build(dependent);
            RegressorCompile = Compile.Build(regressor);
            System.Diagnostics.Debug.WriteLine($"After Compile: {dependentVsRegressor} Mapped: {DependentCompile.numerator}/{DependentCompile.denominator} vs {RegressorCompile.numerator}/{RegressorCompile.denominator}");

            YSelector = InternalCreateSelector(DependentCompile.numerator, DependentCompile.denominator, DependentCompile.token);
            XSelector = InternalCreateSelector(RegressorCompile.numerator, RegressorCompile.denominator, RegressorCompile.token);
            System.Diagnostics.Debug.WriteLine($"After InternalCreate: {dependentVsRegressor} Mapped: {DependentCompile.numerator}/{DependentCompile.denominator} vs {RegressorCompile.numerator}/{RegressorCompile.denominator}");
        }

        public Func<Element, (string id, double x, double y)> Selector
        {
            get
            {
                // ToDo: Add OrdAsc, OrdDesc support

                return e =>
                {
                    var (idx, x) = XSelector(e);
                    var (idy, y) = YSelector(e);

                    return (idx, x, y);
                };
            }
        }

        private Func<Element, (string id, double x)> XSelector { get; }

        private Func<Element, (string id, double y)> YSelector { get; }

        private static Func<Element, (string id, double z)> InternalCreateSelector(string numerator, string denominator, Token token)
        {
            if (token is Token.Ratio or Token.LnRatio)
            {
                return e =>
                {
                    var id = e.Id;
                    var valueN = (double)(GetNestedPropertyValue(e, numerator) ?? double.NaN);
                    var valueD = (double)(GetNestedPropertyValue(e, denominator) ?? double.NaN);

                    // Handle division by zero
                    if (valueN == 0)
                        return (id, 0);

                    //var ratio =
                    //    valueD == 0
                    //    ? valueN / 0.01  // ToDo; make configurable
                    //    : valueN / valueD;

                    //return token == Token.LnRatio
                    //    ? (id, Visit.Ln(ratio)) // Use the stat version of Ln(abs(x) + 1) declared static in Visit class
                    //    : (id, ratio);

                    return token == Token.LnRatio
                        ? (id, Visit.Ln(valueD == 0 ? 0 : valueN / valueD))
                        : (id, valueD == 0 ? 0 : valueN / valueD);

                };
            }
            return e =>
            {
                var id = e.Id;
                var valueN = Convert.ToDouble(GetNestedPropertyValue(e, numerator));
                return (id, valueN);
            };
        }

        public string Title { get; init; }

        public override string ToString()
        {
            return $"{Title}";
        }

        public (Token token, string numerator, string denominator) DependentCompile { get; }

        public (Token token, string numerator, string denominator) RegressorCompile { get; }

        public bool IsDepRegByRank
            => DependentCompile.token == Token.RankA
                                 || DependentCompile.token == Token.RankD
                                 || RegressorCompile.token == Token.RankA
                                 || RegressorCompile.token == Token.RankD
                                 ;
    }
}
