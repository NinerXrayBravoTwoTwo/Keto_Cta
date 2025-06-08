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


        RegressionPvalue CalculateRegression(IEnumerable<Element> targetElements, string label, Func<Element, (double x, double y)> selector)
        {
            var dataPoints = new List<(double x, double y)>();
            dataPoints.AddRange(targetElements.Select(selector));
            var regression = new RegressionPvalue(dataPoints);
            return regression;
        }

        /// <summary>
        /// Regression of Ln(Ncpv) vs Ln(Dcac) for each set.
        /// </summary>
        /// <returns>Regressions for each set, [omega,alpha, zeta, beta, gamma, theta, eta</returns>
        public RegressionPvalue[] MineLnDNcpLnDCac()
        {
            var omega = CalculateRegression(Omega, "Omega", item => (item.LnDNcpv, item.LnDCac));
            var alpha = CalculateRegression(Alpha, "Alpha", item => (item.LnDNcpv, item.LnDCac));
            var zeta = CalculateRegression(Zeta, "Zeta", item => (item.LnDNcpv, item.LnDCac));
            var beta = CalculateRegression(Beta, "Beta", item => (item.LnDNcpv, item.LnDCac));
            var gamma = CalculateRegression(Gamma, "Gamma", item => (item.LnDNcpv, item.LnDCac));
            var theta = CalculateRegression(Theta, "Theta", item => (item.LnDNcpv, item.LnDCac));
            var eta = CalculateRegression(Eta, "Etas", item => (item.LnDNcpv, item.LnDCac));

            return [omega, alpha, zeta, beta, gamma, theta, eta];
        }

        public RegressionPvalue[] MineLnDTpsLnDCac()
        {
            var omega = CalculateRegression(Omega, "Omega", item => (item.LnDTps, item.LnDCac));
            var alpha = CalculateRegression(Alpha, "Alpha", item => (item.LnDTps, item.LnDCac));
            var zeta = CalculateRegression(Zeta, "Zeta", item => (item.LnDTps, item.LnDCac));
            var beta = CalculateRegression(Beta, "Beta", item => (item.LnDTps, item.LnDCac));
            var gamma = CalculateRegression(Gamma, "Gamma", item => (item.LnDTps, item.LnDCac));
            var theta = CalculateRegression(Theta, "Theta", item => (item.LnDTps, item.LnDCac));
            var eta = CalculateRegression(Eta, "Etas", item => (item.LnDTps, item.LnDCac));

            return [omega, alpha, zeta, beta, gamma, theta, eta];

        }

        public RegressionPvalue[] MineLnDPavLnDCac()
        {
            var omega = CalculateRegression(Omega, "Omega", item => (item.LnDPav, item.LnDCac));
            var alpha = CalculateRegression(Alpha, "Alpha", item => (item.LnDPav, item.LnDCac));
            var zeta = CalculateRegression(Zeta, "Zeta", item => (item.LnDPav, item.LnDCac));
            var beta = CalculateRegression(Beta, "Beta", item => (item.LnDPav, item.LnDCac));
            var gamma = CalculateRegression(Gamma, "Gamma", item => (item.LnDPav, item.LnDCac));
            var theta = CalculateRegression(Theta, "Theta", item => (item.LnDPav, item.LnDCac));
            var eta = CalculateRegression(Eta, "Etas", item => (item.LnDPav, item.LnDCac));

            return [omega, alpha, zeta, beta, gamma, theta, eta];

        }

        public RegressionPvalue[] MineLnDTcpvLnDCac()
        {
            var omega = CalculateRegression(Omega, "Omega", item => (item.LnDTcpv, item.LnDNcpv));
            var alpha = CalculateRegression(Alpha, "Alpha", item => (item.LnDTcpv, item.LnDNcpv));
            var zeta = CalculateRegression(Zeta, "Zeta", item => (item.LnDTcpv, item.LnDNcpv));
            var beta = CalculateRegression(Beta, "Beta", item => (item.LnDTcpv, item.LnDNcpv));
            var gamma = CalculateRegression(Gamma, "Gamma", item => (item.LnDTcpv, item.LnDNcpv));
            var theta = CalculateRegression(Theta, "Theta", item => (item.LnDTcpv, item.LnDNcpv));
            var eta = CalculateRegression(Eta, "Etas", item => (item.LnDTcpv, item.LnDNcpv));
            return [omega, alpha, zeta, beta, gamma, theta, eta];
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
