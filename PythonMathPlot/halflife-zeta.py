import pandas as pd
import numpy as np

# Input dataset
data = {
    "Id": [2,51,54,55,56,63,65,66,77,89,94,95],
    "Cac0": [0,6,2,0,0,68,1,58,360,135,291,175],
    "Cac1": [0,0,0,0,0,62,0,39,353,135,351,193],
    "Ncpv0": [193.3,19.6,64.9,68.2,15.5,77.3,113.6,91.1,174.6,46.2,174.9,73.4],
    "Ncpv1": [212.2,42,68.5,71.4,31,96.1,162,119.7,210,41.7,245,92.7]
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