
using Keto_Cta;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DataMiner;

public class CreateSelector
{
    public CreateSelector(string chartTitle)
    {
        var regSplit = Regex.Split(chartTitle, @"\s+vs.\s*", RegexOptions.IgnoreCase);

        if (regSplit.Length < 2)
            throw new ArgumentException("Chart title must contain 'vs.' to separate dependent and independent variables.");

        #region Regressor, source of XSelector  
        var regressor = regSplit[1].Trim();

        // Determine the type of regressor and create the appropriate dicer
        if (regressor.StartsWith("Ln(", StringComparison.OrdinalIgnoreCase) && regressor.EndsWith(")"))
            RegressorDicer = new LnRatioVariableDicer(regressor);
        else if (regressor.Contains('/'))
            RegressorDicer = new RatioVariableDicer(regressor);

        else
            RegressorDicer = new SimpleVariableDicer(regressor);
        #endregion

        #region Dependant, source of YSelector
        DependantDicer = new SimpleVariableDicer(regSplit[0].Trim());
        #endregion

        // Validate that the dependent variable is not the same as the regressor
        if (!IsRatio && RegressorDicer.Target.Equals(DependantDicer.Target, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Dependent and Independent variables must be different: {chartTitle}");

        // Initialize selectors once in the constructor
        XSelector = CreateXSelector();
        _ySelector = CreateYSelector();

    }

    private static readonly Regex PropertyRegex = new(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?", RegexOptions.Compiled);
    private static readonly Dictionary<string, PropertyInfo?> PropertyCache = new();

    public bool IsLogMismatch
    {
        get
        {
            if (RegressorDicer is SimpleVariableDicer simpleRegressor)
                return simpleRegressor.IsLogarithmic != DependantDicer.IsLogarithmic;

            var numLog = Numerator?.IsLogarithmic ?? false;
            var denLog = Denominator?.IsLogarithmic ?? false;
            var depLog = DependantDicer.IsLogarithmic;

            if (IsRatio)
                return numLog != denLog || numLog != depLog;

            if (IsLnWrapRatio)
                return true != depLog;

            return false;
        }
    }

    public bool HasComponentOverlap =>
        !IsRatio && RegressorDicer.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) ||
        IsRatio && (Numerator?.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) == true ||
                    Denominator?.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) == true);

    //public bool IsRatioLnWrapper { get; set; }

    public bool IsRatio => RegressorDicer is RatioVariableDicer;

    public bool IsLnWrapRatio => RegressorDicer is LnRatioVariableDicer;

    public SimpleVariableDicer? Numerator => (RegressorDicer as RatioVariableDicer)?.Numerator ?? (RegressorDicer as LnRatioVariableDicer)?.Numerator;

    public SimpleVariableDicer? Denominator => (RegressorDicer as RatioVariableDicer)?.Denominator ?? (RegressorDicer as LnRatioVariableDicer)?.Denominator;

    public BaseVariableDicer RegressorDicer { get; }

    public SimpleVariableDicer DependantDicer { get; }

    private readonly Func<Element, double> _ySelector;

    public Func<Element, (double, double)> XSelector { get; }

    public Func<Element, double> YSelector => _ySelector;

    private Func<Element, (double, double)> CreateXSelector()
    {
        if (IsRatio || IsLnWrapRatio)
        {
            var numeratorTarget = Numerator?.Target ?? "";
            var denominatorTarget = Denominator?.Target ?? "";
            return e =>
            {
                var numVal = Convert.ToDouble(GetNestedPropertyValue(e, numeratorTarget));
                var denVal = Convert.ToDouble(GetNestedPropertyValue(e, denominatorTarget));
                return (numVal, denVal);
            };
        }
        else
        {
            var regressorTarget = RegressorDicer.Target;
            return e =>
            {
                var val = Convert.ToDouble(GetNestedPropertyValue(e, regressorTarget));
                return (val, 0.0);
            };
        }
    }

    private Func<Element, double> CreateYSelector()
    {
        var dependentTarget = DependantDicer.Target;
        return e =>
        {
            var val = Convert.ToDouble(GetNestedPropertyValue(e, dependentTarget));
            return DependantDicer.IsLogarithmic ? Visit.Ln(val) : val;
        };
    }

    public Func<Element, (double x, double y)> Selector
    {
        get
        {
            return e =>
            {
                var (num, den) = XSelector(e);
                var y = _ySelector(e);
                double x;
                if (IsRatio)
                {
                    x = den != 0 ? num / den : 0;
                }
                else if (IsLnWrapRatio)
                {
                    var ratio = den != 0 ? num / den : 0;
                    x = ratio > 0 ? Visit.Ln(ratio) : double.NaN;
                }
                else
                {
                    x = RegressorDicer.IsLogarithmic ? Visit.Ln(num) : num;
                }
                return (x, y);
            };
        }
    }
    /// <summary>
    /// Retrieves the value of a nested property from an object, following a specified property path.
    /// </summary>
    /// <remarks>This method uses reflection to traverse the property path and retrieve the value.  If any
    /// part of the path is invalid, or if the object or property value is <see langword="null"/>,  the method returns
    /// <see langword="null"/>.</remarks>
    /// <param name="obj">The object from which to retrieve the property value. Can be <see langword="null"/>.</param>
    /// <param name="propertyPath">A dot-separated string representing the path to the nested property.  Supports indexed properties for
    /// collections, e.g., "PropertyName[0].SubProperty".</param>
    /// <returns>The value of the nested property if found; otherwise, <see langword="null"/>.</returns>
    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath) || obj == null)
            return null;

        var properties = propertyPath.Split('.');

        var current = obj;
        foreach (var property in properties)
        {
            if (current == null) return null;

            var match = PropertyRegex.Match(property);
            if (!match.Success) return null;

            var propName = match.Groups[1].Value;
            var cacheKey = $"{current.GetType().FullName}.{propName}";
            if (!PropertyCache.TryGetValue(cacheKey, out var propInfo))
            {
                propInfo = current.GetType().GetProperty(propName);
                PropertyCache[cacheKey] = propInfo;
            }
            if (propInfo == null) return null;

            current = propInfo.GetValue(current);

            if (!match.Groups[2].Success || current is not System.Collections.IList list) continue;
            var index = int.Parse(match.Groups[3].Value);
            current = index >= 0 && index < list.Count ? list[index] : null;
        }
        return current;
    }

    public string ToCsvRow(Element element)
    {
        var (x, y) = Selector(element);
        return $"{RegressorDicer.VariableName},{DependantDicer.VariableName},{x},{y}";
    }
}
