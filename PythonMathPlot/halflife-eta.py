import pandas as pd
import numpy as np

# Input dataset
data = {
    "Id": [62,76,78,79,80,82,83,84,85,86,88,90,93,96,98,99,100],
    "Cac0": [27,69,81,53,66,191,217,17,222,211,88,388,199,556,265,194,221],
    "Cac1": [41,105,97,103,97,218,245,35,253,254,100,400,230,768,322,272,256],
    "Ncpv0": [45.3,53.4,82.4,130.5,78,233.8,169,58.9,244.9,365.6,238.5,147.2,290.8,255.8,200.3,71.8,450.6],
    "Ncpv1": [51.6,112.2,168.4,179,89.6,345.8,182.2,76.4,357.9,428.5,307.3,194.8,378.9,389.6,275.1,103.6,606.5]
}
df = pd.DataFrame(data)

# Calculate doubling times using exponential growth assumption
# growth rate k = ln(C1/C0) / t  => doubling time = ln(2)/k
df["CAC_rate"] = np.log(df["Cac1"]/df["Cac0"]) / 1.0
df["CAC_dt"] = np.log(2) / df["CAC_rate"]

df["NCPV_rate"] = np.log(df["Ncpv1"]/df["Ncpv0"]) / 1.0
df["NCPV_dt"] = np.log(2) / df["NCPV_rate"]

# Compute average doubling times (ignoring negative/0 growth)
cac_dt_mean = df[df["CAC_dt"] > 0]["CAC_dt"].mean()
ncpv_dt_mean = df[df["NCPV_dt"] > 0]["NCPV_dt"].mean()

# Print the results
print("CAC Doubling Time Mean:", cac_dt_mean)
print("NCPV Doubling Time Mean:", ncpv_dt_mean)
print("\nDataFrame with Id, CAC_dt, and NCPV_dt:\n", df[["Id", "CAC_dt", "NCPV_dt"]])