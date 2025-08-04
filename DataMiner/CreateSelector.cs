using System.Text;
using Keto_Cta;
using System.Text.RegularExpressions;

public enum Token
{
    VisitAttribute,
    ElementAttribute,
    Ratio,
    LnRatio
}

namespace DataMiner
{
    public partial class CreateSelector
    {
        public CreateSelector(string dependentVsRegressor)
        {
            // delete any whitespace
            var regressionString = dependentVsRegressor.Trim();

            var vsPattern = @"^(ln\([A-Z\d]+(\s*/\s*[A-Z\d]+)?\)|[A-Z\d]+(\s*/\s*[A-Z\d]+)?)\s+(vs\.?)\s+(ln\([A-Z\d]+(\s*/\s*[A-Z\d]+)?\)|[A-Z\d]+(\s*/\s*[A-Z\d]+)?)\s*$";
            var match = Regex.Match(regressionString, vsPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new ArgumentException($"Invalid regression string format: {regressionString}");

            // group 2 is dependent, group 4 is regressor

            var dependent = match.Groups[1].Value;
            var regressor = match.Groups[5].Value;
            Title = $"{dependent} vs {regressor}";
            System.Diagnostics.Debug.WriteLine($"In: {dependentVsRegressor} Mapped: {Title}");

            DependentCompile = Compile(dependent);
            RegressorCompile = Compile(regressor);

            YSelector = InternalCreateSelector(DependentCompile.numerator, DependentCompile.denominator, DependentCompile.token);
            XSelector = InternalCreateSelector(RegressorCompile.numerator, RegressorCompile.denominator, RegressorCompile.token);

        }

        public Func<Element, (string id, double x, double y)> Selector
        {
            get
            {
                return e =>
                {
                    var (idx, x) = XSelector(e);
                    var (idy, y) = YSelector(e);

                    //System.Diagnostics.Debug.WriteLine($"Element ID: {idx}, X: {x:F3}, Y: {y:F3}");

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
                    var valueN = Convert.ToDouble(GetNestedPropertyValue(e, numerator));
                    var valueD = Convert.ToDouble(GetNestedPropertyValue(e, denominator));


                    // Handle division by zero
                    if (valueN == 0)
                        return (id, 0);

                    var ratio =
                        valueD == 0
                        ? valueN / 0.001
                        : valueN / valueD;

                    return token == Token.LnRatio
                        ? (id, Visit.Ln(ratio)) // Use the stat version of Ln(abs(x) + 1) declared static in Visit class
                        : (id, ratio);
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

        public (Token token, string numerator, string denominator) DependentCompile { get; init; }

        public (Token token, string numerator, string denominator) RegressorCompile { get; init; }


    }
}
