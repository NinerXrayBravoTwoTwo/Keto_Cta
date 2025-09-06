import math
import pandas as pd


rows = [
    # CAC
    ("Cac1/Cac0 vs TdCac","Eta-17",-0.0312,0.5601),
    ("Cac1/Cac0 vs TdCac","Theta-17",-0.0141,0.6968),
    ("Cac1/Cac0 vs TdCac","Beta-34",-0.0137,0.7065),
    ("Cac1/Cac0 vs TdCac","Alpha-82",0.0209,0.8007),
    ("Cac1/Cac0 vs TdCac","Omega-91",0.0151,0.8431),
    ("Cac1/Cac0 vs TdCac","Zeta-9",-0.0067,0.8879),
    ("Cac1/Cac0 vs TdCac","BetaUZeta-43",-0.0030,0.9455),
    # NCPV
    ("Ncpv1/Ncpv0 vs TdNcpv","Gamma-45",-0.1963,0.66116372),
    ("Ncpv1/Ncpv0 vs TdNcpv","Alpha-85",-0.1404,0.62700814),
    ("Ncpv1/Ncpv0 vs TdNcpv","Omega-97",-0.1035,0.67059389),
    ("Ncpv1/Ncpv0 vs TdNcpv","Eta-17",-0.0951,0.38383917),
    ("Ncpv1/Ncpv0 vs TdNcpv","Beta-40",-0.0741,0.53942816),
    ("Ncpv1/Ncpv0 vs TdNcpv","Theta-23",-0.0723,0.55267777),
    ("Ncpv1/Ncpv0 vs TdNcpv","BetaUZeta-52",-0.0489,0.63405439),
    ("Ncpv1/Ncpv0 vs TdNcpv","Zeta-12",-0.0178,0.80121905),
    # QAngio
    ("QAngio1/QAngio0 vs TdQAng","Omega-10",-0.0518,0.81123843),
    ("QAngio1/QAngio0 vs TdQAng","BetaUZeta-9",-0.0543,0.80451432),
    ("QAngio1/QAngio0 vs TdQAng","Alpha-9",-0.0611,0.77587065),
    ("QAngio1/QAngio0 vs TdQAng","Beta-8",-0.0651,0.76433206),
    ("QAngio1/QAngio0 vs TdQAng","Eta-6",-0.0312,0.49525457),

]

df = pd.DataFrame(rows, columns=["Regression","Set","Slope","p_value"])

# classify slope sign
df["Type"] = df["Slope"].apply(lambda s: "Half-life (regression)" if s < 0 else "Doubling (growth)")

# compute time in years
df["Time_years"] = df["Slope"].abs().apply(lambda s: math.log(2)/s if s > 0 else float("inf"))
df["Time_years"] = df["Time_years"].round(2)

print(df)

# save to CSV
df.to_csv("half_life_doubling_times.csv", index=False)