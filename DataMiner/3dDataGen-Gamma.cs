using Keto_Cta;

namespace DataMiner
{
    public partial class GoldMiner
    {
        public string[] PrintOmegaElementsFor3DGammaStudy(SetName setName)
        {
            // LnDPav vs. LnPav0 / LnNcpv0 -- Alpha
            // LnDPav vs. LnPav0 / LnNcpv1 -- Alpha
            // LnDPav vs. LnPav1 / LnNcpv1 -- Alpha

            // LnPav1 vs. LnPav0 / LnNcpv0 -- Alpha
            // LnPav1 vs. LnPav0 / LnNcpv1 -- Alpha

            if (!_setNameToData.TryGetValue(setName, out var elements))
            {
                return [];
            }

            List<string> myData =
            [
                "index,DPav,LnDPav,LnPav0,LnPav1,LnNcpv0,LnNcpv1," +
                "LnPav0/LnNcpv0,LnPav0/LnNcpv1,LnPav1/LnNcpv1,Set"
            ];

            myData.AddRange(elements.Select(elem =>
                $"{elem.Id},{elem.DPav},{elem.LnDPav},{elem.Visits[0].LnPav},{elem.Visits[1].LnPav},{elem.Visits[0].LnNcpv},{elem.Visits[1].LnNcpv}," +
                $"{elem.Visits[0].Pav / elem.Visits[0].Ncpv},{elem.Visits[0].Pav / elem.Visits[1].Ncpv}," +
                $"{elem.Visits[1].LnPav / elem.Visits[1].LnNcpv},{elem.MemberSet}"));

            var ratio0_gamma = new List<string>();
            var ratio1_gamma = new List<string>();
            var ln_pav1_gamma = new List<string>();
            var x_gamma = new List<string>();
            var y_gamma = new List<string>();
            var z_gamma = new List<string>();

            var ratio0_theta = new List<string>();
            var ratio1_theta = new List<string>();
            var ln_pav1_theta = new List<string>();
            var x_theta = new List<string>();
            var y_theta = new List<string>();
            var z_theta = new List<string>();

            var ratio0_eta = new List<string>();
            var ratio1_eta = new List<string>();
            var ln_pav1_eta = new List<string>();
            var x_eta = new List<string>();
            var y_eta = new List<string>();
            var z_eta = new List<string>();

            var ratio0_zeta = new List<string>();
            var ratio1_zeta = new List<string>();
            var ln_pav1_zeta = new List<string>();
            var x_zeta = new List<string>();
            var y_zeta = new List<string>();
            var z_zeta = new List<string>();


            // Build 3D graphic
            //
            // # LnPav0 / LnNcpv0 vs. LnPav1 -- Alpha
            // # Slope; 6.7999 N=88 R^2: 0.9575 p-value: 0.000010 y-int -0.0016
            // 
            // # LnPav0 / LnNcpv1 vs. LnPav1 -- Alpha
            // # Slope; 7.0345 N=88 R^2: 0.9490 p-value: 0.000051 y-int 0.0004
            //
            //  Gamma data
            //# ratio0_gamma = np.array([0.5, 0.6, 0.7, 0.8, 0.9])
            //# ratio1_gamma = np.array([0.5, 0.6, 0.7, 0.8, 0.9])
            //# ln_pav1_gamma = np.array([1.0, 1.5, 2.0, 2.5, 3.0])  # Example data for Gamma
            foreach (var elem in elements)
            {
                if (elem.IsGamma)
                {
                    ratio0_gamma.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    ratio1_gamma.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    ln_pav1_gamma.Add($"{elem.Visits[1].LnPav:F8}");
                    x_gamma.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    y_gamma.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    z_gamma.Add($"{elem.Visits[1].LnPav:F8}");

                    continue;
                }

                if (elem.IsTheta)
                {
                    ratio0_theta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    ratio1_theta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    ln_pav1_theta.Add($"{elem.Visits[1].LnPav:F8}");
                    x_theta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    y_theta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    z_theta.Add($"{elem.Visits[1].LnPav:F8}");

                    continue;
                }

                if (elem.IsEta)
                {
                    ratio0_eta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    ratio1_eta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    ln_pav1_eta.Add($"{elem.Visits[1].LnPav:F8}");
                    x_eta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    y_eta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    z_eta.Add($"{elem.Visits[1].LnPav:F8}");
                    continue;
                }

                if (elem.IsZeta)
                {
                    ratio0_zeta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    ratio1_zeta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    ln_pav1_zeta.Add($"{elem.Visits[1].LnPav:F8}");
                    x_zeta.Add($"{elem.Visits[0].LnPav / elem.Visits[0].LnNcpv:F8}");
                    y_zeta.Add($"{elem.Visits[0].LnPav / elem.Visits[1].LnNcpv:F8}");
                    z_zeta.Add($"{elem.Visits[1].LnPav:F8}");
                    continue;
                }
            }

            myData.Add("\n# 3D Gamma data");
            myData.Add("x_gamma = np.array([" + string.Join(", ", x_gamma) + "])");
            myData.Add("y_gamma = np.array([" + string.Join(", ", y_gamma) + "])");
            myData.Add("z_gamma = np.array([" + string.Join(", ", z_gamma) + "])");

            myData.Add("\n# 3D Theta data");
            myData.Add("x_theta = np.array([" + string.Join(", ", x_theta) + "])");
            myData.Add("y_theta = np.array([" + string.Join(", ", y_theta) + "])");
            myData.Add("z_theta = np.array([" + string.Join(", ", z_theta) + "])");

            myData.Add("\n# 3D Eta data");
            myData.Add("x_eta = np.array([" + string.Join(", ", x_eta) + "])");
            myData.Add("y_eta = np.array([" + string.Join(", ", y_eta) + "])");
            myData.Add("z_eta = np.array([" + string.Join(", ", z_eta) + "])");

            myData.Add("\n# 3D Zeta data");
            myData.Add("x_zeta = np.array([" + string.Join(", ", x_zeta) + "])");
            myData.Add("y_zeta = np.array([" + string.Join(", ", y_zeta) + "])");
            myData.Add("z_zeta = np.array([" + string.Join(", ", z_zeta) + "])");

            // add Gamma data
            myData.Add("\n# Gamma data");
            myData.Add("ratio0_gamma = np.array([" + string.Join(", ", ratio0_gamma) + "])");
            myData.Add("ratio1_gamma = np.array([" + string.Join(", ", ratio1_gamma) + "])");
            myData.Add("ln_pav1_gamma = np.array([" + string.Join(", ", ln_pav1_gamma) + "])");

            myData.Add("\n# Theta data");
            myData.Add("ratio0_theta = np.array([" + string.Join(", ", ratio0_theta) + "])");
            myData.Add("ratio1_theta = np.array([" + string.Join(", ", ratio1_theta) + "])");
            myData.Add("ln_pav1_theta = np.array([" + string.Join(", ", ln_pav1_theta) + "])");

            myData.Add("\n# Eta data");
            myData.Add("ratio0_eta = np.array([" + string.Join(", ", ratio0_eta) + "])");
            myData.Add("ratio1_eta = np.array([" + string.Join(", ", ratio1_eta) + "])");
            myData.Add("ln_pav1_eta = np.array([" + string.Join(", ", ln_pav1_eta) + "])");

            myData.Add("\n# Zeta data");
            myData.Add("ratio0_zeta = np.array([" + string.Join(", ", ratio0_zeta) + "])");
            myData.Add("ratio1_zeta = np.array([" + string.Join(", ", ratio1_zeta) + "])");
            myData.Add("ln_pav1_zeta = np.array([" + string.Join(", ", ln_pav1_zeta) + "])");

            return myData.ToArray();
        }
    }

    public partial class GoldMiner
    {
        public string[] Qangio3DData()
        {
            //Tps0 vs. Ln(Cac0 / DQangio)-- Qangio
            //Slope: 2.9332 N = 10 R ^ 2: 0.9107 p - value: 0.021268 y - int 0.2587

            //Tps0 vs. Ln(Cac1 / DQangio)-- Qangio
            //Slope: 2.8462 N = 10 R ^ 2: 0.9264 p - value: 0.013141 y - int 0.1162


            List<string> myData =
            [
                "index,Tps0,Ln(Cac0/DQangio),Ln(Cac1/DQangio),Set"
            ];


            myData.AddRange(Omega.Select(elem =>
                $"{elem.Id},{elem.Visits[0].Tps},{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)},{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}," +
                $"{elem.MemberSet}"));

            // ********

            var ratio0_gamma = new List<string>();
            var ratio1_gamma = new List<string>();
            var tps0_gamma = new List<string>();
            var x_gamma = new List<string>();
            var y_gamma = new List<string>();
            var z_gamma = new List<string>();

            var ratio0_theta = new List<string>();
            var ratio1_theta = new List<string>();
            var tps0_theta = new List<string>();
            var x_theta = new List<string>();
            var y_theta = new List<string>();
            var z_theta = new List<string>();

            var ratio0_eta = new List<string>();
            var ratio1_eta = new List<string>();
            var tps0_eta = new List<string>();
            var x_eta = new List<string>();
            var y_eta = new List<string>();
            var z_eta = new List<string>();

            var ratio0_zeta = new List<string>();
            var ratio1_zeta = new List<string>();
            var tps0_zeta = new List<string>();
            var x_zeta = new List<string>();
            var y_zeta = new List<string>();
            var z_zeta = new List<string>();

            foreach (var elem in Elements)
            {
                if (elem.IsGamma)
                {
                    ratio0_gamma.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    ratio1_gamma.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    tps0_gamma.Add($"{elem.Visits[0].Tps}");
                    x_gamma.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    y_gamma.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    z_gamma.Add($"{elem.Visits[1].Tps}");

                    continue;
                }

                if (elem.IsTheta)
                {
                    ratio0_theta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    ratio1_theta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    tps0_theta.Add($"{elem.Visits[0].Tps}");
                    x_theta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    y_theta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    z_theta.Add($"{elem.Visits[1].Tps}");

                    continue;
                }

                if (elem.IsEta)
                {
                    ratio0_eta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    ratio1_eta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    tps0_eta.Add($"{elem.Visits[0].Tps}");
                    x_eta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    y_eta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    z_eta.Add($"{elem.Visits[1].Tps}");
                    continue;
                }

                if (elem.IsZeta)
                {
                    ratio0_zeta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    ratio1_zeta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    tps0_zeta.Add($"{elem.Visits[0].Tps}");
                    x_zeta.Add($"{MathUtils.Ln(elem.Visits[0].Cac / elem.DQangio)}");
                    y_zeta.Add($"{MathUtils.Ln(elem.Visits[1].Cac / elem.DQangio)}");
                    z_zeta.Add($"{elem.Visits[1].Tps}");
                    continue;
                }
            }

            /* *************** */
            myData.Add("\n# 3D Gamma data");
            myData.Add("x_gamma = np.array([" + string.Join(", ", x_gamma) + "])");
            myData.Add("y_gamma = np.array([" + string.Join(", ", y_gamma) + "])");
            myData.Add("z_gamma = np.array([" + string.Join(", ", z_gamma) + "])");

            myData.Add("\n# 3D Theta data");
            myData.Add("x_theta = np.array([" + string.Join(", ", x_theta) + "])");
            myData.Add("y_theta = np.array([" + string.Join(", ", y_theta) + "])");
            myData.Add("z_theta = np.array([" + string.Join(", ", z_theta) + "])");

            myData.Add("\n# 3D Eta data");
            myData.Add("x_eta = np.array([" + string.Join(", ", x_eta) + "])");
            myData.Add("y_eta = np.array([" + string.Join(", ", y_eta) + "])");
            myData.Add("z_eta = np.array([" + string.Join(", ", z_eta) + "])");

            myData.Add("\n# 3D Zeta data");
            myData.Add("x_zeta = np.array([" + string.Join(", ", x_zeta) + "])");
            myData.Add("y_zeta = np.array([" + string.Join(", ", y_zeta) + "])");
            myData.Add("z_zeta = np.array([" + string.Join(", ", z_zeta) + "])");

            // add Gamma data
            myData.Add("\n# Gamma data");
            myData.Add("ratio0_gamma = np.array([" + string.Join(", ", ratio0_gamma) + "])");
            myData.Add("ratio1_gamma = np.array([" + string.Join(", ", ratio1_gamma) + "])");
            myData.Add("tps0_gamma = np.array([" + string.Join(", ", tps0_gamma) + "])");

            myData.Add("\n# Theta data");
            myData.Add("ratio0_theta = np.array([" + string.Join(", ", ratio0_theta) + "])");
            myData.Add("ratio1_theta = np.array([" + string.Join(", ", ratio1_theta) + "])");
            myData.Add("tps0_theta = np.array([" + string.Join(", ", tps0_theta) + "])");

            myData.Add("\n# Eta data");
            myData.Add("ratio0_eta = np.array([" + string.Join(", ", ratio0_eta) + "])");
            myData.Add("ratio1_eta = np.array([" + string.Join(", ", ratio1_eta) + "])");
            myData.Add("tps0_eta = np.array([" + string.Join(", ", tps0_eta) + "])");

            myData.Add("\n# Zeta data");
            myData.Add("ratio0_zeta = np.array([" + string.Join(", ", ratio0_zeta) + "])");
            myData.Add("ratio1_zeta = np.array([" + string.Join(", ", ratio1_zeta) + "])");
            myData.Add("tps0_zeta = np.array([" + string.Join(", ", tps0_zeta) + "])");

            return myData.ToArray();
            /***************  */
        }
    }
}