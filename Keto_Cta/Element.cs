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
/// </summary>
internal enum SetSetName
{
    Zeta = 1, // Unicorns
    Gamma = 2, // Zero CAC
    Theta = 3, // Smaller CAC increase  
    Eta = 4 // Larger CAC increase
}

/// <summary>
///     Rename Participant to Element.  Believe it or not there is no actual personal information in the data set.
///     So it is better to represent the class by what it is, an element of the set of all Keto-cta samples,
///     an element of that set 'x' (limit( 1 -&gt; 100)
///     This is a mathematical set, not a database table or a collection of participants
///     Conclusion, a note from our editors;
///     Using “Element” is an excellent choice for your project.
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

        ParticipantId = id ?? throw new ArgumentNullException(nameof(id));

        Visits = visits;
    }

    public string ParticipantId { get; init; }
    public List<Visit> Visits { get; init; }

    // set definitions

    // isZeta(x) = tps2(x) < tps1( x ) OR cac2(x) < cac1(x) "
    // Δcac(x) = cac2(x) - cac1(x)
    //
    // Definitions based on the provided sets and conditions: 

    /*
     • Ω : All participants
         ◦ 100 participants

       • α : { x ∈ Ω | ¬isZeta(x) }
         ◦ 89 participants (CAC and TPS stable or increasing)

       • ζ : { x ∈ Ω | isZeta(x) }
         ◦ 11 participants (CAC or TPS decrease, “Unicorns”)

       • β : { x ∈ α | cac1(x) ≠ 0 ∨ cac2(x) ≠ 0 }
         ◦ 40 participants (non-zero CAC in α)

       • γ : { x ∈ α | cac1(x) = 0 ∧ cac2(x) = 0 }
         ◦ 49 participants (zero CAC in α)

       • η : { x ∈ β | Δcac(x) > 10 }
         ◦ 17 participants (larger CAC increase)

       • θ : { x ∈ β | Δcac(x) ≤ 10 }
         ◦ 23 participants (smaller CAC increase)
     */

    public bool IsZeta => Visits[1].Cac < Visits[0].Cac || Visits[1].Tps < Visits[0].Tps;

    public double DeltaCac => Visits[1].Cac - Visits[0].Cac;

    public bool IsAlpha => !IsZeta;
    public bool IsBeta => IsAlpha && (Visits[0].Cac != 0 || Visits[1].Cac != 0);
    public bool IsGamma => IsAlpha && Visits[0].Cac == 0 && Visits[1].Cac == 0;
    public bool IsEta => IsBeta && DeltaCac > 10;
    public bool IsTheta => IsBeta && DeltaCac <= 10;

    public override string ToString()
    {
        return $"ParticipantId: {ParticipantId}, Set: TBD Visits: [{string.Join(", ", Visits)}]";
    }
}

public record Visit
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Visit" /> class with the specified visit details.
    /// </summary>
    /// <param name="id">The unique identifier for the visit.</param>
    /// <param name="visitDate">
    ///     The date and time of the visit. Can be <see langword="null" /> if the visit date is not
    ///     specified.
    /// </param>
    /// <param name="tps">The total plaque score</param>
    /// <param name="cac">The Coronary Artery Calcium, in Angston units</param>
    /// <param name="ncpv">The Non Calcified Plaque Volume.</param>
    /// <param name="tcpv">The Total calcified plaque volume</param>
    /// <param name="pav">The Percent Atheroma Volume</param>
    public Visit(string id, DateTime? visitDate, double tps, double cac, double ncpv, double tcpv, double pav)
    {
        Id = id;
        VisitDate = visitDate;
        Tps = tps;
        Cac = cac;
        Ncpv = ncpv;
        Tcpv = tcpv;
        Pav = pav;
    }

    public string Id { get; init; }
    public DateTime? VisitDate { get; init; }
    public double Tps { get; init; }
    public double Cac { get; init; }
    public double Ncpv { get; init; }
    public double Tcpv { get; init; }
    public double Pav { get; init; }

    public override string ToString()
    {
        return
            $"Id: {Id}, VisitDate: {VisitDate}, Tps: {Tps:F3}, Cac: {Cac:F3}, Ncpv: {Ncpv:F3}, Tcpv: {Tcpv:F3}, Pav: {Pav:F3}";
    }
}