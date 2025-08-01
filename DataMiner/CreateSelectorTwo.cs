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
            
            StringBuilder sbTitle = new StringBuilder($"n:{DependentCompile.numerator}");
  
            Title = $"{dependent} =vs= {regressor}";

            switch(DependentCompile.token)
            {
                case Token.ElementAttribute:
                case Token.VisitAttribute:
                    YSelector = CreateSelector(DependentCompile.numerator);
                    break;

                case Token.Ratio:
                case Token.LnRatio:
                    YSelector = CreateSelector(DependentCompile.numerator, DependentCompile.denominator);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (RegressorCompile.token)
            {
                case Token.ElementAttribute:
                case Token.VisitAttribute:
                    XSelector = CreateSelector(RegressorCompile.numerator);
                    break;

                case Token.Ratio:
                case Token.LnRatio:
                    XSelector = CreateSelector(RegressorCompile.numerator, RegressorCompile.denominator);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private static Func<Element, (string id, double z)> CreateSelector(string numerator, string denominator = "")
        {
            if (denominator != "")
            {
                return e =>
                {
                    string id = e.Id;
                    double valueN = Convert.ToDouble(GetNestedPropertyValue(e, numerator));
                    double valueD = Convert.ToDouble(GetNestedPropertyValue(e, denominator));


                    // Handle division by zero
                    if (valueN == 0)
                        return (id, 0);

                    return valueD == 0
                        ? (id, valueN / 0.001)
                        : (id, valueN / valueD);
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
