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
    public partial class CreateSelectorTwo
    {
        public CreateSelectorTwo(string dependentVsRegressor)
        {
            // delete any whitespace
            string regressionString = Regex.Replace(dependentVsRegressor, @"\s+", string.Empty);

            var vsPattern = @"((ln)?\(?[A-Za-z\d/]+\)?)\s*vs\.?\s*((ln)?\(?[A-Za-z\d/]+\)?)";
            var match = Regex.Match(regressionString, vsPattern, RegexOptions.IgnoreCase);

            if (!match.Success)
                throw new ArgumentException($"Invalid regression string format: {regressionString}");
            // group 1 is dependent, group 3 is regressor
            // group 2 and 4 are optional ln() wrappers; 2 for dependent, 4 for regressor

            string dependent = match.Groups[2].Length == 0 ? RemoveParens(match.Groups[1].Value) : "Ln(" + RemoveParens(match.Groups[1].Value) + ")";
            string regressor = match.Groups[4].Length == 0 ? RemoveParens(match.Groups[3].Value) : "Ln(" + RemoveParens(match.Groups[3].Value) + ")";

            DependentCompile = Compile(dependent);
            RegressorCompile = Compile(regressor);

            Title = $"{dependent} =vs= {regressor}";

            YSelector = CreateSelector(DependentCompile.numerator, DependentCompile.denominator, DependentCompile.token);
            XSelector = CreateSelector(RegressorCompile.numerator, RegressorCompile.denominator, RegressorCompile.token);

        }

        public Func<Element, (string id, double x, double y)> Selector
        {
            get
            {
                return e =>
                {
                    var (idx, x) = XSelector(e);
                    var (idy, y) = YSelector(e);

                    string message = $"Element ID: {idx}={idy}, X: {x}, Y: {y}";
                    return (idx, x, y);
                };
            }
        }

        public Func<Element, (string id, double x)> XSelector { get; }

        public Func<Element, (string id, double y)> YSelector { get; }

        private static Func<Element, (string id, double z)> CreateSelector(string numerator, string denominator, Token token)
        {
            if (token is Token.Ratio or Token.LnRatio)
            {
                return e =>
                {
                    string id = e.Id;
                    double valueN = Convert.ToDouble(GetNestedPropertyValue(e, numerator));
                    double valueD = Convert.ToDouble(GetNestedPropertyValue(e, denominator));


                    // Handle division by zero
                    if (valueN == 0)
                        return (id, 0);

                    var ratio= 
                        valueD == 0
                        ?  valueN / 0.001
                        :  valueN / valueD;

                    return token == Token.LnRatio 
                        ? (id, Visit.Ln(ratio)) // Use the stat version of Ln(abs(x) + 1) declared static in Visit class
                        : (id, ratio);
                };
            }
            return e =>
            {
                string id = e.Id;
                double valueN = Convert.ToDouble(GetNestedPropertyValue(e, numerator));
                return (id, valueN);
            };
        }

        public override string ToString()
        {
            return $"{Title}";
        }

        public (Token token, string numerator, string denominator) DependentCompile { get; init; }

        public (Token token, string numerator, string denominator) RegressorCompile { get; init; }

        public string Title { get; init; }
    }
}
