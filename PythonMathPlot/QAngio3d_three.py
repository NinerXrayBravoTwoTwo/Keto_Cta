import matplotlib.pyplot as plt
import numpy as np

# Data with IDs
data1 = np.array([
    [2, 0, 0],
    [48, 0, 0],
    [53, 0, 0],
    [82, 1.3941995406270038, 4],
    [85, 0.859493133481022, 4],
    [86, 1.8365734850178477, 4],
    [87, 1.556193397915288, 4],
    [93, 2.4895255956442948, 7],
    [96, 1.6696064339005532, 8],
    [100, 4.31303376318693, 13]
])
data2 = np.array([
    [2, 0, 0],
    [48, 0, 0],
    [53, 0.03174869831458027, 0],
    [82, 1.495219942942213, 4],
    [85, 0.936935059454501, 4],
    [86, 1.9947003132247452, 4],
    [87, 1.5716975844512533, 4],
    [93, 2.6230569882688175, 7],
    [96, 1.939243457697124, 8],
    [100, 4.458215773031428, 13]
])

# Extract coords
ids = data1[:, 0]
x = data1[:, 1]  # Ln(CAC0/ΔQangio)
y = data2[:, 1]  # Ln(CAC1/ΔQangio)
z = data1[:, 2]  # Tps0
disparity = y - x  # ln(CAC1/CAC0) = rate of CAC change

# Scale disparity to match Tps0 range (0-13)
scale_factor = np.max(z) / np.max(np.abs(disparity)) if np.max(np.abs(disparity)) != 0 else 1
z_adjusted = z + disparity * scale_factor * 0.5  # Adjust z with scaled disparity

# 3D Linear regression: Tps0 ~ x + y
X = np.vstack((x, y, np.ones_like(x))).T  # Design matrix [x, y, 1]
coeffs = np.linalg.lstsq(X, z, rcond=None)[0]  # Solve for [a, b, c]
a, b, c = coeffs  # Tps0 = a*x + b*y + c

# Trend plane
x_line = np.linspace(min(x), max(x), 20)
y_line = np.linspace(min(y), max(y), 20)
x_grid, y_grid = np.meshgrid(x_line, y_line)
z_grid = a * x_grid + b * y_grid + c

# Color mapping by ID
color_map = {
    2: 'yellow',   # Zeta
    48: 'blue',    # Gamma
    53: 'purple',  # Theta
    87: 'purple',  # Theta
    82: 'lightgreen', # Eta
    85: 'lightgreen',
    86: 'lightgreen',
    93: 'lightgreen',
    96: 'lightgreen',
    100: 'lightgreen'
}
colors = [color_map[int(i)] for i in ids]

# Plot
fig = plt.figure(figsize=(9, 7))
ax = fig.add_subplot(111, projection='3d')

# Scatter points with colors
ax.scatter(x, y, z_adjusted, c=colors, s=50, label='Data points')

# Vector arrows for CAC change
dx = y - x
dy = dx
dz = disparity * scale_factor * 0.5  # Match z_adjusted scaling
ax.quiver(x, y, z_adjusted, dx, dy, dz, color='red', length=1, normalize=False)

# Trend plane
ax.plot_surface(x_grid, y_grid, z_grid, color='lightblue', alpha=0.5, label='Trend plane')

# Labels
ax.set_xlabel('Ln(CAC0 / ΔQangio)')
ax.set_ylabel('Ln(CAC1 / ΔQangio)')
ax.set_zlabel('Tps0 + Scaled Disparity')
ax.set_title('3D Regression with CAC Acceleration Vectors')
ax.legend()

plt.tight_layout()
plt.show()