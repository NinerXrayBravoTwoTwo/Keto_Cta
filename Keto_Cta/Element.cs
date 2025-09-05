namespace Keto_Cta;

// <summary>
//     The four current leaf sets of the Keto CTA data set in this subset partition
//     set definitions
// 
//       isZeta = v2.Tps < v1.Tps
//            or v2.Cac < v1.Cac
//            or v2.Ncpv < v1.Ncpv
//            or v2.Tcpv < v1.Tcpv
//            or v2.Pav < v1.Pav;
//
// Definitions based on the provided sets and conditions:
//     • Ω : Omega, All participants
//     ◦ 100 participants
//     • α : Alpha, { x ∈ Ω | ¬isZeta(x) }
//     ◦ 88 participants (CAC and TPS stable or increasing)
//     • ζ : Zeta, { x ∈ Ω | isZeta(x) }
//     ◦ 12 participants (CAC or TPS decrease, “Unicorns”)
//     • β : Beta, { x ∈ α | cac1(x) ≠ 0 ∨ cac2(x) ≠ 0 }
//     ◦ 40 participants (non-zero CAC in α)
//     • γ : Gamma, { x ∈ α | cac1(x) = 0 ∧ cac2(x) = 0 }
//     ◦ 4 participants (zero CAC in α)
//     • η : Eta, { x ∈ β | Δcac(x) &gt; 10 }
//     ◦ 17 participants (larger CAC increase)
//     • θ : Theta, { x ∈ β | Δcac(x) ≤ 10 }
//     ◦ 23 participants (smaller CAC increase)
// </summary>
public enum LeafSetName
{
    Zeta = 1, // Unicorns
    Gamma = 2, // Zero CAC
    Theta = 3, // Smaller CAC increase  
    Eta = 4 // Larger CAC increase
}

public enum SetName
{
    Omega = 0, // All participants, Zeta U Gamma U Theta U Eta
    Alpha = 5, // Non-Zeta participants
    Beta = 6, // Non-Zeta with non-zero CAC, Alpha Except Beta
    Zeta = 1, // Unicorns
    Gamma = 2, // Zero CAC in Alpha, Alpha exclude Gamma
    Theta = 3, // Smaller CAC increase in Beta
    Eta = 4, // Larger CAC increase in Beta
    BetaUZeta = 7,  // Beta union Zeta Eta and Theta are closely related to the Zeta set in terms of CAC
}

// <summary>
//     “Element”
//     It depersonalizes the data, avoids implying transactional state,
//     and aligns with the mathematical framework of your subset partition.
//     The updated code reflects this change while maintaining the same
//     functionality, ensuring clarity and precision in your terminology.
//     I think this will make your project easier to reason about as it grows.
//     It also avoids the potential confusion of using a term like "Participant" when the
//     data does not actually contain personal information.
// </summary>
public class Element
{
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="visits"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Element(string id, List<Visit> visits)
    {
        ArgumentNullException.ThrowIfNull(visits);

        if (visits.Count < 2)
            throw new ArgumentException("Visits list must contain at least two visits.", nameof(visits));

        Id = id ?? throw new ArgumentNullException(nameof(id));

        Visits = visits;
        MemberSet = ComputeSetState(visits[0], visits[1]);

        DTps = MathUtils.Diff(Visits[0].Tps, Visits[1].Tps);
        DCac = MathUtils.Diff(Visits[0].Cac, Visits[1].Cac);
        DNcpv = MathUtils.Diff(Visits[0].Ncpv, Visits[1].Ncpv);
        DTcpv = MathUtils.Diff(Visits[0].Tcpv, Visits[1].Tcpv);
        DPav = MathUtils.Diff(Visits[0].Pav, Visits[1].Pav);
        DQangio = MathUtils.Diff(Visits[0].Qangio, Visits[1].Qangio); // Handle NaN if needed

        GeoMeanTps = MathUtils.GeoMean(Visits.Select(v => v.Tps).ToArray());
        GeoMeanCac = MathUtils.GeoMean(Visits.Select(v => v.Cac).ToArray());
        GeoMeanNcpv = MathUtils.GeoMean(Visits.Select(v => v.Ncpv).ToArray());
        GeoMeanTcpv = MathUtils.GeoMean(Visits.Select(v => v.Tcpv).ToArray());
        GeoMeanPav = MathUtils.GeoMean(Visits.Select(v => v.Pav).ToArray());
        GeoMeanQangio = MathUtils.GeoMean(Visits.Select(v => v.Qangio).ToArray());

        LnDTps = MathUtils.Ln(DTps);
        LnDCac = MathUtils.Ln(DCac);
        LnDNcpv = MathUtils.Ln(DNcpv);
        LnDTcpv = MathUtils.Ln(DTcpv);
        LnDPav = MathUtils.Ln(DPav);
        LnDQangio = MathUtils.Ln(DQangio);

        LnGeoMeanTps = MathUtils.Ln(GeoMeanTps);
        LnGeoMeanCac = MathUtils.Ln(GeoMeanCac);
        LnGeoMeanNcpv = MathUtils.Ln(GeoMeanNcpv);
        LnGeoMeanTcpv = MathUtils.Ln(GeoMeanTcpv);
        LnGeoMeanPav = MathUtils.Ln(GeoMeanPav);
        LnGeoMeanQangio = MathUtils.Ln(GeoMeanQangio);

        TdTps = MathUtils.Td(Visits[0].Tps, Visits[1].Tps);
        TdCac = MathUtils.Td(Visits[0].Cac, Visits[1].Cac);
        TdNcpv = MathUtils.Td(Visits[0].Ncpv, Visits[1].Ncpv);
        TdTcpv = MathUtils.Td(Visits[0].Tcpv, Visits[1].Tcpv);
        TdPav = MathUtils.Td(Visits[0].Pav, Visits[1].Pav);
        TdQangio = MathUtils.Td(Visits[0].Qangio, Visits[1].Qangio); // Will return NaN if invalid

        MaxNcpv = MathUtils.Max(Visits.Select(v => v.Ncpv).ToArray());
        LnMaxNcpv = MathUtils.Ln(MaxNcpv);
        
    }

    public double MaxNcpv { get; init; }
    public double LnMaxNcpv { get; init; }

    public double GeoMeanTps { get; init; }
    public double GeoMeanCac { get; init; }
    public double GeoMeanNcpv { get; init; }
    public double GeoMeanTcpv { get; init; }
    public double GeoMeanPav { get; init; }
    public double GeoMeanQangio { get; init; }

    public double LnDTps { get; init; }
    public double LnDCac { get; init; }
    public double LnDNcpv { get; init; }
    public double LnDTcpv { get; init; }
    public double LnDPav { get; init; }
    public double LnDQangio { get; init; }

    public double LnGeoMeanTps { get; init; }
    public double LnGeoMeanCac { get; init; }
    public double LnGeoMeanNcpv { get; init; }
    public double LnGeoMeanTcpv { get; init; }
    public double LnGeoMeanPav { get; init; }
    public double LnGeoMeanQangio { get; init; }
    
    public LeafSetName MemberSet { get; init; }

    public string Id { get; init; }
    public List<Visit> Visits { get; init; }

    public double DTps { get; init; }
    public double DCac { get; init; }
    public double DNcpv { get; init; }
    public double DTcpv { get; init; }
    public double DPav { get; init; }
    public double DQangio { get; init; }
    public double TdTps { get; init; }
    public double TdCac { get; init; }
    public double TdNcpv { get; init; }
    public double TdTcpv { get; init; }
    public double TdPav { get; init; }
    public double TdQangio { get; init; }

    //LnTdCac is not valid since Td vars are already Ln transformed

    public bool IsBeta => IsAlpha && (Visits[0].Cac != 0 || Visits[1].Cac != 0);
    public bool IsAlpha => MemberSet != LeafSetName.Zeta; // Not a Unicorn

    public bool IsZeta => MemberSet == LeafSetName.Zeta;
    public bool IsGamma => MemberSet == LeafSetName.Gamma;
    public bool IsTheta => MemberSet == LeafSetName.Theta; // Smaller CAC increase
    public bool IsEta => MemberSet == LeafSetName.Eta; // Larger CAC increase

    private static LeafSetName ComputeSetState(Visit v1, Visit v2)
    {
        if (
             v2.Tps < v1.Tps
            || v2.Cac < v1.Cac
            || v2.Ncpv < v1.Ncpv
            || v2.Tcpv < v1.Tcpv
            || v2.Pav < v1.Pav
            )
            return LeafSetName.Zeta; // Unicorns

        if (v1.Cac == 0 && v2.Cac == 0) return LeafSetName.Gamma; // Zero CAC

        return (v2.Cac - v1.Cac) switch // Delta CAC
        {
            > 10 => LeafSetName.Eta,
            <= 10 => LeafSetName.Theta,
            _ => throw new NotImplementedException()
        };
    }

    public override string ToString()
    {
        // T_d values are -infinity  half life and +infinity for +0 T_d

        var repVisits = $"{Visits[0]}\n{Visits[1]}";
        return $"Id: {Id} {MemberSet}:,, DTps {DTps:F3}, DCac {DCac:F3}, DNcpv {DNcpv:F3}, DTcpv {DTcpv:F3}, DPav {DPav:F3}, DQAng {DQangio:F3}"
               + $"\nhalf-life: ,, TdCac {TdCac:F4}, TdNcpv {TdNcpv:F4}, TdTps {TdTps:f4}, TdQangio {TdQangio:F4}"
               + $"\n{repVisits}";
    }
}