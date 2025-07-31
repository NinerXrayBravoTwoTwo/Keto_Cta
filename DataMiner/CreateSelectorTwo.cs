using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Reflection;
using Keto_Cta;

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
        public CreateSelectorTwo(string regressionString)
        {
            var split = Regex.Split(regressionString, @"\s+vs(.)\s*", RegexOptions.IgnoreCase);
            string dependent = split[0].Trim();
            string regressor = split[1].Trim();

            var compileDependent = Compile(dependent);
            var compileRegressor = Compile(regressor);

            switch (compileRegressor.token)
            {
                case Token.ElementAttribute:
                case Token.VisitAttribute:
                    XSelector = CreateSelector(compileRegressor.numerator);
                    YSelector = CreateSelector(compileDependent.numerator);

                    break;
                    
                case Token.Ratio:
                case Token.LnRatio:
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

        private static Func<Element, (string id, double z)> CreateSelector(string attribute)
        {
            return e =>
            {
                string id = e.Id;
                double value = Convert.ToDouble(GetNestedPropertyValue(e, attribute));
                return (id, value);
            };
        }

        //private Func<Element, (double x, double y)> CreateSelector(string attribute)
        //{
        //    return e =>
        //    {
        //        // Implement the logic to extract x and y values from the element
        //        // This is a placeholder implementation
        //        double x = Convert.ToDouble(GetNestedPropertyValue(e, attribute));
        //        double y = 0; // Replace with actual logic to get y value
        //        return (x, y);
        //    };
        //}

        //public Func<Element, (string id, double x, double y)> Selector
        //{
        //    get
        //    {
        //        string id;
        //        double x;
        //        double y;
        //        return e =>
        //        {
        //            id = e.Id;
        //            x = XSelector // Replace with actual logic to get x value
        //            y = 0; // Replace with actual logic to get y value
        //            return (id, x, y);
        //        };
        //    }
        //}
    }

