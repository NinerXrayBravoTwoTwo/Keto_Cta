using Keto_Cta;

namespace DataMiner;


public class RankSelector()
{
    public RankSelector(CreateSelector selector, Element[] elements) : this()
    {
        Selector = selector;
        Elements = elements;

        IsRegRank = selector.RegressorCompile.token is Token.RankA or Token.RankD;
        IsDepRank = selector.DependentCompile.token is Token.RankA or Token.RankD;

        if (IsRegRank && IsDepRank)
            throw new ArgumentException("Both Dependent and Regressor can not request ordinal status");

        if (!(IsRegRank || IsDepRank))
            throw new ArgumentException("The Regressor or Dependent must be an Rank to use this object");

        IsAsc = IsRegRank && selector.RegressorCompile.token is Token.RankA
                    || IsDepRank && selector.DependentCompile.token is Token.RankA;

         RawDataPoints = elements.Select(selector.Selector).ToArray();

        if (IsRegRank)
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

    public bool IsDepRank { get; set; }

    public bool IsRegRank { get; set; }
    
    public bool IsRank => IsRegRank || IsDepRank;

    public CreateSelector Selector { get; set; }

    public Element[] Elements { get; set; }

    public (string id, double x, double y)[] RawDataPoints { get; set; }

    public (string id, double x, double y)[] DataPoints { get; set; }
}

