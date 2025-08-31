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
    Qangio = 8,  // Where element.DQangio not double.NaN
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
    }

    public LeafSetName MemberSet { get; init; }

    public string Id { get; init; }
    public List<Visit> Visits { get; set; }

    // Move it inside constructor to ensure it is computed once, it is outside for temporary testing
    public double DTps => Visits[1].Tps - Visits[0].Tps;
    public double DCac => Visits[1].Cac - Visits[0].Cac;
    public double DNcpv => Visits[1].Ncpv - Visits[0].Ncpv;
    public double DTcpv => Visits[1].Tcpv - Visits[0].Tcpv;
    public double DPav => Visits[1].Pav - Visits[0].Pav;
    public double DQangio => Visits[1].Qangio - Visits[0].Qangio;

    public double LnDTps => Visit.Ln(DTps);
    public double LnDCac => Visit.Ln(DCac);
    public double LnDNcpv => Visit.Ln(DNcpv);
    public double LnDTcpv => Visit.Ln(DTcpv);
    public double LnDPav => Visit.Ln(DPav);
    public double LnDQangio => Visit.Ln(DQangio);
    
    /*
       if v2 > v1:  # growth
           return dt * math.log(2) / math.log(v2 / v1)
       elif v2 < v1:  # regression
           return -(dt * math.log(2) / math.log(v2 / v1))  # negative = half-life
       else:
           return None  # no change
     */
    public double GrowthCac => DCac == 0 ? 0 : (DCac > 0
            ? (1 * Math.Log(2) / Math.Log(Visits[1].Cac / Visits[0].Cac))
            : -(1 * Math.Log(2) / Math.Log(Visits[0].Cac / Visits[1].Cac)));

    public double GrowthNcpv => DNcpv == 0 ? 0 : (DNcpv > 0
            ? (1 * Math.Log(2) / Math.Log(Visits[1].Ncpv / Visits[0].Ncpv))
            : -(1 * Math.Log(2) / Math.Log(Visits[0].Ncpv / Visits[1].Ncpv)));

    public double GrowthQangio => DQangio == 0 ? 0 : (DQangio > 0
            ? (1 * Math.Log(2) / Math.Log(Visits[1].Qangio / Visits[0].Qangio))
            : -(1 * Math.Log(2) / Math.Log(Visits[0].Qangio / Visits[1].Qangio)));

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
        var repVisits = $"{Visits[0]}\n{Visits[1]}";
        return $"Id: {Id} {MemberSet}:,, DTps {DTps:F3}, DCac {DCac:F3}, DNcpv {DNcpv:F3}, DTcpv {DTcpv:F3}, DPav {DPav:F3}, DQAng {DQangio:F3}"
               + $"\n{repVisits}";
    }
}