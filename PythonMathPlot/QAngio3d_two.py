import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# Tps0 vs. Ln(Cac0 / DQangio)-- Qangio
# Slope: 2.9332 N = 10 R ^ 2: 0.9107 p - value: 0.021268 y - int 0.2587

# Tps0 vs. Ln(Cac1 / DQangio)-- Qangio
# Slope: 2.8462 N = 10 R ^ 2: 0.9264 p - value: 0.013141 y - int 0.1162

# 3D Gamma data
x_gamma = np.array([0])
y_gamma = np.array([0])
z_gamma = np.array([1])

# 3D Theta data
x_theta = np.array([0, 1.556193397915288])
y_theta = np.array([0.03174869831458027, 1.5716975844512533])
z_theta = np.array([3, 6])

# 3D Eta data
x_eta = np.array(
    [
        1.3941995406270038,
        0.859493133481022,
        1.8365734850178477,
        2.4895255956442948,
        1.6696064339005532,
        4.31303376318693,
    ]
)
y_eta = np.array(
    [
        1.495219942942213,
        0.936935059454501,
        1.9947003132247452,
        2.6230569882688175,
        1.939243457697124,
        4.458215773031428,
    ]
)
z_eta = np.array([4, 5, 6, 7, 8, 14])

# 3D Zeta data
x_zeta = np.array([0])
y_zeta = np.array([0])
z_zeta = np.array([0])


# Gamma data
ratio0_gamma = np.array([0])
ratio1_gamma = np.array([0])
tps0_gamma = np.array([0])

# Theta data
ratio0_theta = np.array([0, 1.556193397915288])
ratio1_theta = np.array([0.03174869831458027, 1.5716975844512533])
tps0_theta = np.array([0, 4])

# Eta data
ratio0_eta = np.array(
    [
        1.3941995406270038,
        0.859493133481022,
        1.8365734850178477,
        2.4895255956442948,
        1.6696064339005532,
        4.31303376318693,
    ]
)
ratio1_eta = np.array(
    [
        1.495219942942213,
        0.936935059454501,
        1.9947003132247452,
        2.6230569882688175,
        1.939243457697124,
        4.458215773031428,
    ]
)
tps0_eta = np.array([4, 4, 4, 7, 8, 13])

# Zeta data
ratio0_zeta = np.array([0])
ratio1_zeta = np.array([0])
tps0_zeta = np.array([0])

# Regression params
slope1, intercept1 = 2.9332, 0.2587
slope2, intercept2 = 2.8462, 0.1162

ids = np.concatenate([x_zeta, x_theta, x_eta, x_gamma])  # Combine IDs if needed
x = np.concatenate([ratio0_zeta, ratio0_theta, ratio0_eta, ratio0_gamma])  # Ln(Cac0 / DQangio)
y = np.concatenate([ratio1_zeta, ratio1_theta, ratio1_eta, ratio1_gamma])  # Ln(Cac1 / DQangio)
z = np.concatenate([tps0_zeta, tps0_theta, tps0_eta, tps0_gamma])  # Tps0

# Synthesized trend line
x_line = np.linspace(min(x), max(x), 50)
y_line = np.linspace(min(y), max(y), 50)
z_line = (slope1 * x_line + intercept1 + slope2 * y_line + intercept2) / 2


# Matrix transform
def matrix_project(left_x, right_x, y, b=0.5):
    disparity = left_x - right_x
    proj_matrix = np.array(
        [[1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, -1 / b, 1]]
    )
    disparity = left_x - right_x
    point_left = np.array([left_x, y, 0, 1])
    projected = proj_matrix @ point_left
    projected[2] += disparity * 40  # Adjust depth with disparity
    return projected[0], projected[1], projected[2], disparity


# Transform data to 3D points
x_zeta_proj, y_zeta_proj, z_zeta_proj, disparity_zeta = np.vectorize(matrix_project)(
    ratio0_zeta, ratio1_zeta, tps0_zeta
)
x_theta_proj, y_theta_proj, z_theta_proj, disparity_theta = np.vectorize(
    matrix_project
)(ratio0_theta, ratio1_theta, tps0_theta)
x_eta_proj, y_eta_proj, z_eta_proj, disparity_eta = np.vectorize(matrix_project)(
    ratio0_eta, ratio1_eta, tps0_eta
)
x_gamma_proj, y_gamma_proj, z_gamma_proj, disparity_gamma = np.vectorize(
    matrix_project
)(ratio0_gamma, ratio1_gamma, tps0_gamma)

# Plot without masking zeros (points at Z=0 if disparity=0)
fig = plt.figure(figsize=(12, 10))
ax = fig.add_subplot(111, projection="3d")

# Plot Zeta
ax.scatter(
    x_zeta_proj,
    y_zeta_proj,
    z_zeta_proj,
    c="orange",
    marker="D",
    label="ζ Zeta - regressing",
)

# Plot Theta
ax.scatter(
    x_theta_proj,
    y_theta_proj,
    z_theta_proj,
    c="purple",
    marker="o",
    label="θ Theta - CAC",
)

# Plot Eta
ax.scatter(
    x_eta_proj, y_eta_proj, z_eta_proj, c="green", marker="s", label="η Eta - CAC++"
)

# Plot Gamma
ax.scatter(
    x_gamma_proj,
    y_gamma_proj,
    z_gamma_proj,
    c="blue",
    marker="^",
    label="γ Gamma - no CAC",
)

# # Highlight your point (in Theta)
# user_x = 0.772404
# user_index_theta = np.argmin(np.abs(ratio0_theta - user_x))
# ax.scatter(x_theta_proj[user_index_theta], y_theta_proj[user_index_theta], z_theta_proj[user_index_theta], c='magenta', marker='*', s=150, label='Your Point')


# Add vectors for movement (length determined by magnitude of U,V,W; no 'length' param)
arrow_length_scale = 0.5  # Adjust to make arrows shorter/longer
mask_theta = disparity_theta != 0  # Skip zeros
ax.quiver(
    x_theta_proj[mask_theta],
    y_theta_proj[mask_theta],
    z_theta_proj[mask_theta],
    disparity_theta[mask_theta] * arrow_length_scale,
    0,
    disparity_theta[mask_theta] * arrow_length_scale,
    color="purple",
    arrow_length_ratio=0.3,
)

# Repeat for Eta
mask_eta = disparity_eta != 0
ax.quiver(
    x_eta_proj[mask_eta],
    y_eta_proj[mask_eta],
    z_eta_proj[mask_eta],
    disparity_eta[mask_eta] * arrow_length_scale,
    0,
    disparity_eta[mask_eta] * arrow_length_scale,
    color="green",
    arrow_length_ratio=0.3,
)

# Repeat for Zeta
mask_zeta = disparity_zeta != 0
ax.quiver(
    x_zeta_proj[mask_zeta],
    y_zeta_proj[mask_zeta],
    z_zeta_proj[mask_zeta],
    disparity_zeta[mask_zeta] * arrow_length_scale,
    0,
    disparity_zeta[mask_zeta] * arrow_length_scale,
    color="orange",
    arrow_length_ratio=0.3,
)

mask_gamma = disparity_gamma != 0
ax.quiver(
    x_gamma_proj[mask_gamma],
    y_gamma_proj[mask_gamma],
    z_gamma_proj[mask_gamma],
    disparity_gamma[mask_gamma] * arrow_length_scale,
    0,
    disparity_gamma[mask_gamma] * arrow_length_scale,
    color="blue",
    arrow_length_ratio=0.3,
)

# Trend line
ax.plot(x_line, y_line, z_line, color="lightblue", linewidth=2, label="trend line")

ax.set_xlabel("Ln(Cac0 / DQangio)")
ax.set_ylabel("Ln(Cac1 / DQangio")
ax.set_zlabel("Tps1")
ax.set_title("Cac Velocity Chart with vectors")
ax.legend()

plt.show()
