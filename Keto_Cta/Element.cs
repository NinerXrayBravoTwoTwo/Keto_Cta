namespace Keto_Cta;
/* Example JSON structure for IParticipant
      * {
            "id": "d4e5f",  // Hash of V1 data: "009.300.004"
            "visits": [
              {
                "id": "V1",
                "visit_date": null,
                "tps": 0,
                "cac": 0,
                "ncpv": 9.3,
                "tcpv": 0,
                "pav": 0.004
              },
              {
                "id": "V2",
                "visit_date": null,
                "tps": 0,
                "cac": 0,
                "ncpv": 18.8,
                "tcpv": 0,
                "pav": 0.007
              }
            ]
          },
      */

/// <summary>
///     The four current leaf sets of the Keto CTA data set in this subset partition
///     set definitions
///     isZeta(x) = tps2(x) &lt; tps1( x ) OR cac2(x) &lt; cac1(x) "
///     Δcac(x) = cac2(x) - cac1(x)
///     Definitions based on the provided sets and conditions:
///     • Ω : All participants
///     ◦ 100 participants
///     • α : { x ∈ Ω | ¬isZeta(x) }
///     ◦ 89 participants (CAC and TPS stable or increasing)
///     • ζ : { x ∈ Ω | isZeta(x) }
///     ◦ 11 participants (CAC or TPS decrease, “Unicorns”)
///     • β : { x ∈ α | cac1(x) ≠ 0 ∨ cac2(x) ≠ 0 }
///     ◦ 40 participants (non-zero CAC in α)
///     • γ : { x ∈ α | cac1(x) = 0 ∧ cac2(x) = 0 }
///     ◦ 49 participants (zero CAC in α)
///     • η : { x ∈ β | Δcac(x) &gt; 10 }
///     ◦ 17 participants (larger CAC increase)
///     • θ : { x ∈ β | Δcac(x) ≤ 10 }
///     ◦ 23 participants (smaller CAC increase)
/// </summary>
public enum SetName
{
    Zeta = 1, // Unicorns
    Gamma = 2, // Zero CAC
    Theta = 3, // Smaller CAC increase  
    Eta = 4 // Larger CAC increase
}

/// <summary>
///     “Element”
///     It depersonalizes the data, avoids implying transactional state,
///     and aligns with the mathematical framework of your subset partition.
///     The updated code reflects this change while maintaining the same
///     functionality, ensuring clarity and precision in your terminology.
///     I think this will make your project easier to reason about as it grows.
///     It also avoids the potential confusion of using a term like "Participant" when the
///     data does not actually contain personal information.
/// </summary>
public record Element
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

    public SetName MemberSet { get; init; }

    public string Id { get; init; }
    public List<Visit> Visits { get; init; }

    // Move it inside constructor to ensure it is computed once, it is outside for temporary testing
    public double DCac => Visits[1].Cac - Visits[0].Cac;
    public double DNcpv => Visits[1].Ncpv - Visits[0].Ncpv;
    public double DTcp => Visits[1].Tcpv - Visits[0].Tcpv;
    public double DPav => Visits[1].Pav - Visits[0].Pav;

    public  double Ln(double value) => Math.Log(Math.Abs(value) + 1, double.E);

    public double LnDCac => Ln(DCac);
    public double LnDNcpv => Ln(DNcpv);
    public double LnDTcpv => Ln(DTcp);
    public double LnDPav => Ln(DPav);


    public bool IsBeta => IsAlpha && (Visits[0].Cac != 0 || Visits[1].Cac != 0);
    public bool IsAlpha => MemberSet != SetName.Zeta; // Not a Unicorn

    public bool IsZeta => MemberSet == SetName.Zeta;
    public bool IsGamma => MemberSet == SetName.Gamma;
    public bool IsTheta => MemberSet == SetName.Theta; // Smaller CAC increase
    public bool IsEta => MemberSet == SetName.Eta; // Larger CAC increase

    //public SetName SetOf { get; init; } = ComputeSetState(Visits[0], Visits[1]);

    private SetName ComputeSetState(Visit v1, Visit v2)
    {
        var cac1 = v1.Cac;
        var tps1 = v1.Tps;

        var cac2 = v2.Cac;
        var tps2 = v2.Tps;

        if (cac2 < cac1 || tps2 < tps1) return SetName.Zeta; // Unicorns

        if (cac1 == 0 && cac2 == 0) return SetName.Gamma; // Zero CAC

        return (cac2 - cac1) switch // Delta CAC
        {
            > 10 => SetName.Eta,
            <= 10 => SetName.Theta
        };
    }

    public override string ToString()
    {
        return $"ParticipantId: {Id}, Set: {MemberSet} Visits: [{string.Join(", ", Visits)}]";
    }
}