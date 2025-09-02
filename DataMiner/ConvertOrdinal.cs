using Keto_Cta;

namespace DataMiner;


public class ConvertOrdinal()
{
    public ConvertOrdinal(CreateSelector selector, Element[] elements) : this()
    {
        Selector = selector;
        Elements = elements;

        IsRegOrd = selector.RegressorCompile.token is Token.OrdAsc or Token.OrdDesc;
        IsDepOrd = selector.DependentCompile.token is Token.OrdAsc or Token.OrdDesc;

        if (IsRegOrd && IsDepOrd)
            throw new ArgumentException("Both Dependent and Regressor can not request ordinal status");

        if (!(IsRegOrd || IsDepOrd))
            throw new ArgumentException("The Regressor or Dependent must be an Ordinal to use this object");

        IsAsc = IsRegOrd && selector.RegressorCompile.token is Token.OrdAsc
                    || IsDepOrd && selector.DependentCompile.token is Token.OrdAsc;

         RawDataPoints = elements.Select(selector.Selector).ToArray();

        if (IsRegOrd)
            DataPoints = IsAsc
                 ? RawDataPoints.OrderBy(t => t.y)
                    .Select((t, index) => (t.id, x: (double)(index + 1), t.y))
                    .ToArray()
                : RawDataPoints.
                   OrderByDescending(t => t.y)
                    .Select((t, index) => (t.id, x: (double)(index + 1), t.y))
                    .ToArray();
        else
            DataPoints = IsAsc
                ? RawDataPoints.OrderBy(t => t.x)
                    .Select((t, index) => (t.id, t.x, y: (double)(index + 1)))
                    .ToArray()
                : RawDataPoints.
                    OrderByDescending(t => t.x)
                    .Select((t, index) => (t.id, t.x, y: (double)(index + 1)))
                    .ToArray();

    }

    public bool IsAsc { get; set; }

    public bool IsDepOrd { get; set; }

    public bool IsRegOrd { get; set; }

    public CreateSelector Selector { get; set; }

    public Element[] Elements { get; set; }

    public (string id, double x, double y)[] RawDataPoints { get; set; }

    public (string id, double x, double y)[] DataPoints { get; set; }
}

