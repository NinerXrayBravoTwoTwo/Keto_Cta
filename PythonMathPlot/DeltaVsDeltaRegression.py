import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import linregress

# Theta arrays (N=23)
x_theta = np.array([
    2.674148649, 2.90690106, 3.56671182, 3.254242969, 4.611152258,
    3.61361697, 3.148453361, 1.223775432, 3.958906591, 2.721295428,
    3.723280881, 3.841600541, 2.433613355, 4.039536326, 2.468099531,
    2.734367509, 3.929862924, 2.370243741, 4.446174454, 4.391976966,
    3.443618098, 4.225372825, 4.127134385
])

y_theta = np.array([
    0.693147181, 1.098612289, 0.693147181, 0.693147181, 0.693147181,
    1.098612289, 0, 1.609437912, 1.609437912, 1.609437912,
    1.386294361, 0, 2.197224577, 1.098612289, 1.791759469,
    2.397895273, 1.791759469, 2.397895273, 0.693147181, 1.098612289,
    1.791759469, 1.791759469, 2.079441542
])

# Eta arrays (N=17)
x_eta = np.array([
    1.987874348, 4.091005661, 4.465908119, 3.90197267, 2.533696814,
    4.727387819, 2.653241965, 2.917770732, 4.736198448, 4.157319361,
    4.24563401, 3.883623531, 4.489759334, 4.903792198, 4.328098293,
    3.490428515, 5.05560866
])

y_eta = np.array([
    2.708050201, 3.610917913, 2.833213344, 3.931825633, 3.465735903,
    3.33220451, 3.36729583, 2.944438979, 3.465735903, 3.784189634,
    2.564949357, 2.564949357, 3.465735903, 5.361292166, 4.060443011,
    4.369447852, 3.583518938
])

# Zeta arrays (N=12)
x_zeta = np.array([
    2.990719732, 3.152736022, 1.526056303, 1.435084525, 2.803360381,
    2.985681938, 3.899950424, 3.387774361, 3.594568775, 1.704748092,
    4.264087337, 3.010620886
])

y_zeta = np.array([
    0, 1.945910149, 1.098612289, 0, 0, 1.945910149, 0.693147181,
    2.995732274, 2.079441542, 0, 4.110873864, 2.944438979
])

# Create DataFrames
theta_df = pd.DataFrame({'ln_delta_NCPV': x_theta, 'ln_delta_CAC': y_theta})
eta_df = pd.DataFrame({'ln_delta_NCPV': x_eta, 'ln_delta_CAC': y_eta})
zeta_df = pd.DataFrame({'ln_delta_NCPV': x_zeta, 'ln_delta_CAC': y_zeta})

# Beta as union of Theta and Eta (optional, but not plotted per request)
beta_df = pd.concat([theta_df, eta_df], ignore_index=True)

# Function to compute regression if valid points exist
def get_regression(x, y):
    mask = (x > 0) & (y > 0)
    x_valid = x[mask]
    y_valid = y[mask]
    if len(x_valid) < 2:
        return None, None, None
    res = linregress(x_valid, y_valid)
    return res.slope, res.intercept, res.rvalue**2

# Create single plot
plt.figure(figsize=(10, 8))

# Plot Theta (purple points and line)
mask_theta = (theta_df['ln_delta_NCPV'] > 0) & (theta_df['ln_delta_CAC'] > 0)
x_theta_valid = theta_df['ln_delta_NCPV'][mask_theta]
y_theta_valid = theta_df['ln_delta_CAC'][mask_theta]
plt.scatter(x_theta_valid, y_theta_valid, color='purple', marker='o', label='Theta (N=' + str(len(x_theta_valid)) + ')')
slope_theta, intercept_theta, r2_theta = get_regression(theta_df['ln_delta_NCPV'], theta_df['ln_delta_CAC'])
if slope_theta is not None:
    plt.plot(x_theta_valid, intercept_theta + slope_theta * x_theta_valid, color='purple', label=f'Theta Slope: {slope_theta:.3f}, R²: {r2_theta:.3f}')

# Plot Eta (green points and line)
mask_eta = (eta_df['ln_delta_NCPV'] > 0) & (eta_df['ln_delta_CAC'] > 0)
x_eta_valid = eta_df['ln_delta_NCPV'][mask_eta]
y_eta_valid = eta_df['ln_delta_CAC'][mask_eta]
plt.scatter(x_eta_valid, y_eta_valid, color='green', marker='s', label='Eta (N=' + str(len(x_eta_valid)) + ')')
slope_eta, intercept_eta, r2_eta = get_regression(eta_df['ln_delta_NCPV'], eta_df['ln_delta_CAC'])
if slope_eta is not None:
    plt.plot(x_eta_valid, intercept_eta + slope_eta * x_eta_valid, color='green', label=f'Eta Slope: {slope_eta:.3f}, R²: {r2_eta:.3f}')

# Plot Zeta (orange points and line)
mask_zeta = (zeta_df['ln_delta_NCPV'] > 0) & (zeta_df['ln_delta_CAC'] > 0)
x_zeta_valid = zeta_df['ln_delta_NCPV'][mask_zeta]
y_zeta_valid = zeta_df['ln_delta_CAC'][mask_zeta]
plt.scatter(x_zeta_valid, y_zeta_valid, color='orange', marker='D', label='Zeta (N=' + str(len(x_zeta_valid)) + ')')
slope_zeta, intercept_zeta, r2_zeta = get_regression(zeta_df['ln_delta_NCPV'], zeta_df['ln_delta_CAC'])
if slope_zeta is not None:
    plt.plot(x_zeta_valid, intercept_zeta + slope_zeta * x_zeta_valid, color='orange', label=f'Zeta Slope: {slope_zeta:.3f}, R²: {r2_zeta:.3f}')

plt.xlabel('ln(ΔNCPV + 1)')
plt.ylabel('ln(ΔCAC + 1)')
plt.title('Combined Delta Regressions: Zeta, Theta, Eta')
plt.legend()
plt.grid(True)
plt.show()