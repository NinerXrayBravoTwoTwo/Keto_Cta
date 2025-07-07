using Keto_Cta;
using System.Text.RegularExpressions;

namespace DataMiner;

public class CreateSelector
{
    public bool IsLogMismatch => (RegressorDicer is SimpleVariableDicer simpleRegressor && DependantDicer is SimpleVariableDicer simpleDependant)
        ? simpleRegressor.IsLogarithmic != simpleDependant.IsLogarithmic
        : IsRatio && (Numerator?.IsLogarithmic != Denominator?.IsLogarithmic ||
                      (Numerator?.IsLogarithmic != DependantDicer.IsLogarithmic && !DependantDicer.IsDelta) ||
                      (Denominator?.IsLogarithmic != DependantDicer.IsLogarithmic && !DependantDicer.IsDelta));
    public bool IsUninteresting => HasComponentOverlap || IsLogMismatch;
    public bool HasComponentOverlap =>
        !IsRatio && RegressorDicer.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) ||
        IsRatio && (Numerator?.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) == true ||
                    Denominator?.Target.Contains(DependantDicer.Target, StringComparison.OrdinalIgnoreCase) == true);
    public bool IsRatio => RegressorDicer is RatioVariableDicer;
    public SimpleVariableDicer? Numerator => (RegressorDicer as RatioVariableDicer)?.Numerator;
    public SimpleVariableDicer? Denominator => (RegressorDicer as RatioVariableDicer)?.Denominator;
    public BaseVariableDicer RegressorDicer { get; }
    public SimpleVariableDicer DependantDicer { get; }
    private readonly Func<Element, (double, double)> _xSelector;
    private readonly Func<Element, double> _ySelector;

    public Func<Element, (double, double)> XSelector => _xSelector;
    public Func<Element, double> YSelector => _ySelector;

    public CreateSelector(string chartTitle)
    {
        var regSplit = Regex.Split(chartTitle, @"\s+vs.\s*", RegexOptions.IgnoreCase);
        if (regSplit.Length < 2)
            throw new ArgumentException("Chart title must contain 'vs.' to separate dependent and independent variables.");

        RegressorDicer = regSplit[0].Contains('/')
            ? new RatioVariableDicer(regSplit[0].Trim())
            : new SimpleVariableDicer(regSplit[0].Trim());

        DependantDicer = new SimpleVariableDicer(regSplit[1].Trim());

        if (!IsRatio && RegressorDicer.Target.Equals(DependantDicer.Target, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Dependent and Independent variables must be different: {chartTitle}");

        // Initialize selectors once in the constructor
        _xSelector = CreateXSelector();
        _ySelector = CreateYSelector();
    }

    private Func<Element, (double, double)> CreateXSelector()
    {
        if (IsRatio)
        {
            var numeratorTarget = Numerator?.Target ?? "";
            var denominatorTarget = Denominator?.Target ?? "";
            return e => (
                Convert.ToDouble(GetNestedPropertyValue(e, numeratorTarget)),
                Convert.ToDouble(GetNestedPropertyValue(e, denominatorTarget))
            );
        }
        else
        {
            var regressorTarget = RegressorDicer.Target;
            return e => (
                Convert.ToDouble(GetNestedPropertyValue(e, regressorTarget)),
                0.0 // Placeholder for non-ratio case; adjust if needed
            );
        }
    }

    private Func<Element, double> CreateYSelector()
    {
        var dependentTarget = DependantDicer.Target;
        return e => Convert.ToDouble(GetNestedPropertyValue(e, dependentTarget));
    }

    public Func<Element, (double x, double y)> Selector 
    {
        get
        {
            if (IsRatio)
            {
                return e =>
                {
                    var (numerator, denominator) = _xSelector(e);
                    var y = _ySelector(e);
                    return (denominator != 0 ? numerator / denominator : 0, y);
                };
            }
            else
            {
                return e =>
                {
                    var (x, _) = _xSelector(e);
                    var y = _ySelector(e);
                    return (x, y);
                };
            }
        }
    }

    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath) || obj == null)
            return null;

        var regex = new Regex(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?");
        var properties = propertyPath.Split('.');

        if (properties.Length == 1)
        {
            var propInfo = obj.GetType().GetProperty(propertyPath);
            return propInfo?.GetValue(obj);
        }

        foreach (var property in properties)
        {
            if (obj == null) return null;

            var match = regex.Match(property);
            if (!match.Success) return null;

            var propName = match.Groups[1].Value;
            var propInfo = obj.GetType().GetProperty(propName);
            if (propInfo == null) return null;

            obj = propInfo.GetValue(obj);

            if (match.Groups[2].Success && obj is System.Collections.IList list)
            {
                int index = int.Parse(match.Groups[3].Value);
                if (index < 0 || index >= list.Count) return null;
                obj = list[index];
            }
        }
        return obj;
    }
}