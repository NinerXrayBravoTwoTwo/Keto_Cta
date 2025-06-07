using Keto_Cta;
using LinearRegression;

namespace DataMiner
{
    public class SaltMiner
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SaltMiner" /> class.
        /// </summary>
        public SaltMiner(string path)
        {
            var elements = ReadCsvFile(path);

            // Load elements into sets based on their MemberSet property

            Omega = elements.Where(e => e.MemberSet is SetName.Zeta or SetName.Gamma or SetName.Theta or SetName.Eta).ToArray();
            Alpha = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta or SetName.Gamma).ToArray();
            Beta = elements.Where(e => e.MemberSet is SetName.Theta or SetName.Eta).ToArray();
            Zeta = elements.Where(e => e.MemberSet == SetName.Zeta).ToArray();
            Gamma = elements.Where(e => e.MemberSet == SetName.Gamma).ToArray();
            Theta = elements.Where(e => e.MemberSet == SetName.Theta).ToArray();
            Eta = elements.Where(e => e.MemberSet == SetName.Eta).ToArray();

        }

        public Element[] Omega;
        public Element[] Alpha;
        public Element[] Beta;
        public Element[] Zeta;
        public Element[] Gamma;
        public Element[] Theta;
        public Element[] Eta;

        /// <summary>
        ///     This method is a placeholder for future functionality.
        /// </summary>
        public RegressionPvalue[]  MineLnNcpLnDcac()
        {
            // Placeholder for salt mining logic
            Console.WriteLine("Mining salt...");

            var omega = RegressionLnNcpvLnDcac(Omega, "Omega");
            var alpha = RegressionLnNcpvLnDcac(Alpha, "Alpha");
            var zeta = RegressionLnNcpvLnDcac(Zeta, "Zeta");
            var beta = RegressionLnNcpvLnDcac(Beta, "Beta");
            var gamma = RegressionLnNcpvLnDcac(Gamma, "Gamma");
            var theta = RegressionLnNcpvLnDcac(Theta, "Theta");
            var eta = RegressionLnNcpvLnDcac(Eta, "Etas");

            return [omega,alpha,zeta,beta,gamma, theta, eta];


            RegressionPvalue RegressionLnNcpvLnDcac(IEnumerable<Element> targetElements, string label)
            {
                var dataPoints = new List<(double x, double y)>();

                dataPoints.AddRange(targetElements.Select(item => (item.LnDNcpv, LnDCp: item.LnDCac)));

                var regression = new RegressionPvalue(dataPoints);

                return regression;
            }
        }

        private static List<Element> ReadCsvFile(string path)
        {
            var list = new List<Element>();
            if (list == null) throw new ArgumentNullException(nameof(list));

            using var reader = new StreamReader(path);
            var index = 0;
            // Skip the header line
            if (!reader.EndOfStream) reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var values = line.Split(',');
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var visit1 = new Visit("V1", null, int.Parse(values[0]), int.Parse(values[2]), double.Parse(values[4]),
                    double.Parse(values[6]), double.Parse(values[8]));
                var visit2 = new Visit("V2", null, int.Parse(values[1]), int.Parse(values[3]), double.Parse(values[5]),
                    double.Parse(values[7]), double.Parse(values[9]));

                index++;
                var element = new Element(index.ToString(), [visit1, visit2]);

                list.Add(element);
            }

            return list;
        }
    }
}
