namespace Keto_Cta;

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
    /// <param name="qangio">The QAngio value, default is 0.0</param>
    public Visit(string id, DateTime? visitDate, int tps, int cac, double ncpv, double tcpv, double pav, double qangio=double.NaN)
    {
        Id = id;
        VisitDate = visitDate;
        Tps = tps;
        Cac = cac;
        Ncpv = ncpv;
        Tcpv = tcpv;
        Pav = pav;
        QAngio = qangio;

        LnTps = Ln(tps);
        LnCac = Ln(cac);
        LnNcpv = Ln(ncpv);
        LnTcpv = Ln(tcpv);
        LnPav = Ln(pav);
        LnQAngio =Ln(QAngio);
    }

    public static double Ln(double value)
    {
        return Math.Log(Math.Abs(value) + 1, double.E);
    }

    public string Id { get; init; }
    public DateTime? VisitDate { get; init; }
    public double Tps { get; init; }
    public double Cac { get; init; }
    public double Ncpv { get; init; }
    public double Tcpv { get; init; }
    public double Pav { get; init; }
    public double QAngio { get; init; } = 0.0;

    public double LnTps { get; init; }
    public double LnCac { get; init; }
    public double LnNcpv { get; init; }
    public double LnTcpv { get; init; }
    public double LnPav { get; init; }

    public double LnQAngio { get; init; } = 0.0;
    public override string ToString()
    {
        return
            $"Id: {Id}, VisitDate: {VisitDate}, Tps: {Tps:F3}, Cac: {Cac:F3}, Ncpv: {Ncpv:F3}, Tcpv: {Tcpv:F3}, Pav: {Pav:F3}";
    }
}