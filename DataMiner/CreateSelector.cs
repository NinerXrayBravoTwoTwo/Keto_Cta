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
    public Func<Element, (double x, double y)> Selector { get; }
    public Func<Element, (double numerator, double denominator)> XSelector =>
        e => (
            Convert.ToDouble(GetNestedPropertyValue(e, Numerator?.Target ?? "")),
            Convert.ToDouble(GetNestedPropertyValue(e, Denominator?.Target ?? ""))
        );
    public Func<Element, double> YSelector =>
        e => Convert.ToDouble(GetNestedPropertyValue(e, DependantDicer.Target));

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

        Selector = item =>
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Element cannot be null.");

            double xValue;
            if (IsRatio)
            {
                var ratioDicer = (RatioVariableDicer)RegressorDicer;
                var numeratorValue = GetNestedPropertyValue(item, ratioDicer.Numerator.Target);
                var denominatorValue = GetNestedPropertyValue(item, ratioDicer.Denominator.Target);
                if (numeratorValue == null || denominatorValue == null)
                    throw new ArgumentException($"Properties '{ratioDicer.Numerator.Target}' or '{ratioDicer.Denominator.Target}' not found in Element.");

                var num = Convert.ToDouble(numeratorValue);
                var denom = Convert.ToDouble(denominatorValue);
                xValue = denom != 0 ? num / denom : 0;
            }
            else
            {
                var regressorValue = GetNestedPropertyValue(item, RegressorDicer.Target);
                if (regressorValue == null)
                    throw new ArgumentException($"Property '{RegressorDicer.Target}' not found in Element.");
                xValue = Convert.ToDouble(regressorValue);
            }

            var yValue = GetNestedPropertyValue(item, DependantDicer.Target);
            if (yValue == null)
                throw new ArgumentException($"Property '{DependantDicer.Target}' not found in Element.");

            return (xValue, Convert.ToDouble(yValue));
        };
    }

    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath) || obj == null)
            return null;

        var regex = new Regex(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?");
        var properties = propertyPath.Split('.');

        if (properties.Length == 1)
        {
            // Direct Element property (e.g., LnDCac, DCac)
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