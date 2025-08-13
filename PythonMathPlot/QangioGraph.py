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
x = data1[:, 1]  # Ln(Cac0/ΔQangio)
y = data2[:, 1]  # Ln(Cac1/ΔQangio)
z = data1[:, 2]  # Tps0

# Regression params
slope1, intercept1 = 2.9332, 0.2587
slope2, intercept2 = 2.8462, 0.1162

# Synthesized trend line
x_line = np.linspace(min(x), max(x), 50)
y_line = np.linspace(min(y), max(y), 50)
z_line = (slope1 * x_line + intercept1 + slope2 * y_line + intercept2) / 2

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
ax.scatter(x, y, z, c=colors, s=50, label='Data points')

# Vector arrows for CAC change
dx = y - x
dy = dx
dz = np.zeros_like(dx)
ax.quiver(x, y, z, dx, dy, dz, color='red', length=1, normalize=False)

# Trend line
ax.plot(x_line, y_line, z_line, color='lightblue', linewidth=2, label='trend line')

# Labels
ax.set_xlabel('Ln(Cac0 / ΔQangio)')
ax.set_ylabel('Ln(Cac1 / ΔQangio)')
ax.set_zlabel('Tps0')
ax.set_title('Regression Pair showing rate of CAC acceleration vectors')
ax.legend()

plt.tight_layout()
plt.show()
