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
        #region log-transformed regressions

        /// <summary>
        /// Regression of Ln(Ncpv) vs Ln(Dcac) for each set.
        /// </summary>
        /// <returns>Regressions for each set, [omega,alpha, zeta, beta, gamma, theta, eta</returns>
        public RegressionPvalue[] MineLnDNcpLnDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.LnDNcpv, item.LnDCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineLnDTpsLnDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.LnDTps, item.LnDCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector); ;
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineLnDPavLnDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.LnDPav, item.LnDCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];

        }

        public RegressionPvalue[] MineLnDTcpvLnDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.LnDTcpv, item.LnDCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }
        #endregion

        #region Delta regressions not log-transformed

        public RegressionPvalue[] MineDNcpvDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.DNcpv, item.DCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineDTpsDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.DTps, item.DCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineDPavDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.DPav, item.DCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineDTcpvDCac()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.DTcpv, item.DCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        #endregion

        #region log-transformed regressions CTA1 vs CTA2

        public RegressionPvalue[] MineLnNcpv1LnNcpv2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].LnNcpv, item.Visits[1].LnNcpv));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }


        public RegressionPvalue[] MineLnTps1LnTps2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].LnTps, item.Visits[1].LnTps));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineLnPav1LnPav2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].LnPav, item.Visits[1].LnPav));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineLnTcpv1LnTcpv2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].LnTcpv, item.Visits[1].LnTcpv));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineLnCac1LnCac2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].LnCac, item.Visits[1].LnCac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }
        #endregion

        #region regressions CTA1 vs CTA2

        public RegressionPvalue[] MineNcpv1Ncpv2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].Ncpv, item.Visits[1].Ncpv));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineTps1Tps2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].Tps, item.Visits[1].Tps));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta]; }

        public RegressionPvalue[] MinePav1Pav2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].Pav, item.Visits[1].Pav));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector); 
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineTcpv1Tcpv2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].Tcpv, item.Visits[1].Tcpv));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
           var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }

        public RegressionPvalue[] MineCac1Cac2()
        {
            var selector = new Func<Element, (double x, double y)>(item => (item.Visits[0].Cac, item.Visits[1].Cac));
            var omega = CalculateRegression(Omega, "Omega", selector);
            var alpha = CalculateRegression(Alpha, "Alpha", selector);
            var zeta = CalculateRegression(Zeta, "Zeta", selector);
            var beta = CalculateRegression(Beta, "Beta", selector);
            var gamma = CalculateRegression(Gamma, "Gamma", selector);
            var theta = CalculateRegression(Theta, "Theta", selector);
            var eta = CalculateRegression(Eta, "Etas", selector);
            var betaUZeta = CalculateRegression(Beta.Concat(Zeta), "BetaUZeta", selector); // Combined Beta and Zeta for specific analysis
            return [omega, alpha, zeta, beta, gamma, theta, eta, betaUZeta];
        }
        #endregion

        /// <summary>
        /// Reads a CSV file from the specified path and parses its contents into a list of <see cref="Element"/> objects.
        /// </summary>
        /// <remarks>The method assumes the CSV file has a header row, which is skipped during processing. Each row in the
        /// file is expected to contain numeric and floating-point values in a specific format. Ensure the file adheres to the
        /// expected structure to avoid parsing errors.</remarks>
        /// <param name="path">The file path of the CSV file to read. Must be a valid path to an existing file.</param>
        /// <returns>A list of <see cref="Element"/> objects, each representing a row in the CSV file.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="path"/> is null.</exception>
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
