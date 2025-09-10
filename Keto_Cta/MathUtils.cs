namespace Keto_Cta;

public static class MathUtils
{
    /// <summary>
    /// Natural logarithm with adjustment for zero/negative values (adds a constant before logging).
    /// Base is e; customize the constant or base as needed.
    ///
    /// Common logarithm function, so if you want to use base 10 instead of e, use Math.Log10
    /// if you want to use a different add constant change the + 1 to something else i.e 0.5 or 1.5 be creative
    /// The AI will have an opinion on a constant, but will never have the joy of comparing 30k regression p-values at a +1
    /// with 30k regression p-values at a +0.5 or +1.5 and saying "Hmm interesting ..." with one eyebrow raised 
    /// </summary>
    public static double Ln(double value, double addConstant = 1.0, double logBase = Math.E)
    {
        return Math.Log(Math.Abs(value) + addConstant, logBase);
    }

    /// <summary>
    /// Geometric mean of two values (sqrt(v1 * v2)).
    /// Handles negatives/zeros by taking absolutes; returns NaN if invalid.
    /// </summary>
    public static double GeoMean(double v1, double v2)
    {
        if (v1 < 0 || v2 < 0) return double.NaN; // Or throw exception if preferred
        return Math.Sqrt(Math.Abs(v1) * Math.Abs(v2));
    }

    /// <summary>
    /// Time to double (positive) or half-life (negative) assuming dt=1 (e.g., years between visits).
    /// Returns 0 if no change; handles v1/v2 order for growth/regression.
    /// </summary>
    public static (double Td,  bool isInfinite) Td(double v1, double v2, double dt = 1.0)
    {

        // infinite time to double or half life is actually just a signal of no change to the v2 value
        if (Math.Abs(v1 - v2) < 0.00000001) return (td: 0.0, isInfinite: true);

        // And into the weeds we go ... :) Log ratios are undefined for zero/negative values, use inverse sqr time for those cases
        if (v1 <= 0 || v2 <= 0) return (td:double.NaN, isInfinite: false); // Invalid for log ratio

        double ratio = v2 / v1;

        // Growth: time to double
        var result = ratio > 1
        ? dt * Math.Log(2) / Math.Log(ratio)

        // Regression: half-life (negative)
        : -(dt * Math.Log(2) / Math.Log(1 / ratio)); // Equivalent to log(v1/v2)
        return (result,false);
    }

    public static double DblPredict((double T_d, bool isInfinit) td, double baseline, double years = 1)
    {
        // Todo: Or, if the half life is infinite I could use MaxValue(var + 1 StdDev) or something like that

        // Td of NaN means invalid, return NaN
        if (double.IsNaN(td.T_d)) return double.NaN;

        // Td of zero means no change, return baseline.
        if (td.T_d == 0) return baseline;

        // compute ratio growth/decay rate
        var r = double.IsPositive(td.T_d)
            ? Math.Log(2, double.E) / td.T_d
            : Math.Log(0.5, double.E) / -td.T_d; // half-life case

        return baseline * Math.Pow(double.E, years * r);
    }

    public static double Min(double v1, double v2) => Math.Min(v1, v2);
    public static double Max(double v1, double v2) => Math.Max(v1, v2);
    public static double Sum(double v1, double v2) => v1 + v2;
    public static double Diff(double v1, double v2) => v2 - v1; // v2 - v1 for delta
    public static double Avg(double v1, double v2) => (v1 + v2) / 2;

    // Overloads for arrays if needed (e.g., for future multi-visit support)
    public static double Min(params double[] values) => values.Min();
    public static double Max(params double[] values) => values.Max();
    public static double Sum(params double[] values) => values.Sum();
    public static double Avg(params double[] values) => values.Average();

    public static double GeoMean(params double[] values)
    {
        if (values.Any(v => v < 0)) return double.NaN;
        double product = values.Aggregate(1.0, (acc, v) => acc * Math.Abs(v));
        return Math.Pow(product, 1.0 / values.Length);
    }

    //// Half-life as a separate wrapper (positive value for regression cases)
    //public static double HalfLife(double v1, double v2, double dt = 1.0)
    //{
    //    double td = Td(v1, v2, dt);
    //    return td < 0 ? -td : double.NaN; // Only meaningful for regression
    //}
}
