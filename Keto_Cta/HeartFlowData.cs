namespace Keto_Cta
{
    public record HeartflowData
    {
        public HeartflowData(int id, double heartflow1, double heartflow2)
        {
            Id = id;
            Heartflow1 = heartflow1;
            Heartflow2 = heartflow2;
        }
        public int Id { get; init; }
        public double Heartflow1 { get; init; } = 0.0;
        public double Heartflow2 { get; init; } = 0.0;
    }
}