namespace Keto_Cta
{
    public record QAngio
    {
        public QAngio(int id, double qangio1, double qangio2)
        {
            Id = id;
            QAngio1 = qangio1;
            QAngio2 = qangio2;
        }
        public int Id { get; init; }
        public double QAngio1 { get; init; } = 0.0;
        public double QAngio2 { get; init; } = 0.0;
    }
}