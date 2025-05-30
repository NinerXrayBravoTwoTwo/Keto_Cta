namespace Keto_Cta
{
    /* Example JSON structure for IParticipant
          * {
                "participant_id": "d4e5f",  // Hash of V1 data: "009.300.004"
                "hash": "a1b2c",
                "visits": [
                  {
                    "visit_id": "V1",
                    "visit_date": null,
                    "tps": 0,
                    "cac": 0,
                    "ncpv": 9.3,
                    "tcpv": 0,
                    "pav": 0.004
                  },
                  {
                    "visit_id": "V2",
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

    public class Participant
    {
        public Participant(string participantId, string hash, List<Visit> visits)
        {
            ArgumentNullException.ThrowIfNull(visits);

            if (visits.Count < 2)
                throw new ArgumentException("Visits list must contain at least two visits.", nameof(visits));

            ParticipantId = participantId ?? throw new ArgumentNullException(nameof(participantId));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            Visits = visits;
        }

        public string ParticipantId { get; }
        public string Hash { get; }
        public List<Visit> Visits { get; }

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
        public bool IsGamma => IsAlpha && (Visits[0].Cac == 0 && Visits[1].Cac == 0);
        public bool IsEta => IsBeta && DeltaCac > 10;
        public bool IsTheta => IsBeta && DeltaCac <= 10;
        
        public override string ToString()
        {
            return $"ParticipantId: {ParticipantId}, Hash: {Hash}, Zeta?: {IsZeta} Alpha?: {IsAlpha} Visits: [{string.Join(", ", Visits)}]";
        }

    }

    public class Visit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Visit"/> class with the specified visit details.
        /// </summary>
        /// <param name="visitId">The unique identifier for the visit.</param>
        /// <param name="visitDate">The date and time of the visit. Can be <see langword="null"/> if the visit date is not specified.</param>
        /// <param name="tps">The total plaque score</param>
        /// <param name="cac">The Coronary Artery Calcium, in Angston units</param>
        /// <param name="ncpv">The Non Calcified Plaque Volume.</param>
        /// <param name="tcpv">The Total calcified plaque volume</param>
        /// <param name="pav">The Percent Atheroma Volume</param>
        public Visit(string visitId, DateTime? visitDate, double tps, double cac, double ncpv, double tcpv, double pav)
        {
            VisitId = visitId;
            VisitDate = visitDate;
            Tps = tps;
            Cac = cac;
            Ncpv = ncpv;
            Tcpv = tcpv;
            Pav = pav;
        }

        public string VisitId { get; }
        public DateTime? VisitDate { get; }
        public double Tps { get; }
        public double Cac { get; }
        public double Ncpv { get; }
        public double Tcpv { get; }
        public double Pav { get; }

        public override string ToString()
        {
            return
                $"VisitId: {VisitId}, VisitDate: {VisitDate}, Tps: {Tps:F3}, Cac: {Cac:F3}, Ncpv: {Ncpv:F3}, Tcpv: {Tcpv:F3}, Pav: {Pav:F3}";
        }


    }
}
