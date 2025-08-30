import pandas as pd
import numpy as np

# Input dataset
data = {
    "Id": [17,49,50,52,53,57,59,61,64,67,68,69,70,71,72,73,74,75,81,87,91,92,97],
    "Cac0": [0,0,0,0,0,4,37,9,63,4,0,21,10,3,12,39,2,21,80,101,119,295,47],
    "Cac1": [1,2,1,1,1,6,37,13,67,8,3,21,18,5,17,49,7,31,81,103,124,300,54],
    "Ncpv0": [6.3,39.7,26.3,20.8,67.1,94.7,42.4,12.8,53.3,139.4,171.5,53.7,52.1,164.2,290.2,83.3,141.1,106.8,163.9,100.5,183.5,252.3,105.7],
    "Ncpv1": [19.8,57,60.7,45.7,166.7,130.8,64.7,15.2,104.7,153.6,211.9,99.3,62.5,220,301,97.7,191,116.5,248.2,180.3,213.8,319.7,166.7]
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