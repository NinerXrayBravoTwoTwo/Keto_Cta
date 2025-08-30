import pandas as pd
import numpy as np

# Input dataset
data = {
    "Id": [1,3,4,5,6,7,8,9,10,11,12,13,14,15,16,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,58,60],
    "Cac0": [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    "Cac1": [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    "Ncpv0": [9.3,6.5,1.8,15.6,10.4,48.4,65.6,3.8,42.8,21.7,4.9,27.8,22.4,26.5,0,8,0,45.7,13.5,1.7,21,5.7,16.5,0,23.3,5.3,101.1,9.9,3.3,0,15.4,24,28.3,20.7,2.2,12.4,23.6,17.2,24.2,1.4,36.7,18.9,3.8,20,61.8,26.6,82.8,4.4],
    "Ncpv1": [18.8,23.2,9.1,24.8,11.8,80.4,82.5,13.1,64.4,44.4,16.7,39.3,55.9,70.9,0,15.3,13.6,64,20.7,6.8,67.3,9.9,58.4,9.2,25.7,5.3,156.6,39.4,9,3.9,25.7,40.4,58,26.1,9.7,23,44.3,25.4,35.3,4.6,80.2,21.9,4,32.8,98.3,104,144.9,11.2]
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