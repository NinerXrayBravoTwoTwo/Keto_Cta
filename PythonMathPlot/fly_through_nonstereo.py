import numpy as np
import matplotlib
matplotlib.use('Agg')  # Use non-interactive Agg backend for saving
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.animation as animation

# Function to compute stereo 3D points
def compute_stereo_3d(ratio0, ratio1, ln_cac1, k=60.0):
    disparity = ratio0 - ratio1
    depth = k * disparity
    x_3d = (ratio0 + ratio1) / 2
    y_3d = ln_cac1
    z_3d = depth
    return x_3d, y_3d, z_3d

# Zeta data (regressors)
ratio0_zeta = np.array([0, 0.6432141900737977, 0.26231517988512754, 0, 0, 0.9710033899612511, 0.1461889298737325, 0.9015366321328295, 1.1394427589104996, 1.2745596383334394, 1.0980360579681494, 1.1997997113370333])
ratio1_zeta = np.array([0, 0.5173641628202729, 0.25902561928345197, 0, 0, 0.9253378102738982, 0.1360779687331669, 0.8506729246131384, 1.100342686869395, 1.3085760744476183, 1.0311375010237032, 1.1388484961156078])
ln_cac1_zeta = np.array([0, 0, 0, 0, 0, 4.143134726391533, 0, 3.6888794541139363, 5.869296913133774, 4.912654885736052, 5.863631175598097, 5.267858159063328])
x_zeta_3d, y_zeta_3d, z_zeta_3d = compute_stereo_3d(ratio0_zeta, ratio1_zeta, ln_cac1_zeta)

# Theta data (low/zero CAC increase)
ratio0_theta = np.array([0, 0, 0, 0, 0, 0.35285263872984957, 0.9647593924678355, 0.877286031413336, 1.0411460407002446, 0.3255009363788492, 0, 0.7724007307085876, 0.6036728262897569, 0.27144150822594215, 0.45205229008535625, 0.8318813233527058, 0.22164943200951182, 0.6604399736330077, 0.8607555653155671, 1.0010636260309163, 0.9175571828446315, 1.0281475831090183, 0.8289472102329216])
ratio1_theta = np.array([0, 0, 0, 0, 0, 0.3297159881728276, 0.8691756693182583, 0.8267776654084305, 0.8923483493991499, 0.31927963388710684, 0, 0.6707750237222871, 0.5776613396057578, 0.25680855464584623, 0.4491694490897574, 0.8033125469454092, 0.20896115182777275, 0.6485015093091424, 0.7963474917371539, 0.8893916599278506, 0.891574050402815, 0.9861109838733201, 0.7557726453415707])
ln_cac1_theta = np.array([0.6931471805599453, 1.0986122886681098, 0.6931471805599453, 0.6931471805599453, 0.6931471805599453, 1.9459101490553132, 3.6375861597263857, 2.6390573296152584, 4.219507705176107, 2.1972245773362196, 1.3862943611198906, 3.091042453358316, 2.9444389791664403, 1.791759469228055, 2.8903717578961645, 3.912023005428146, 2.0794415416798357, 3.4657359027997265, 4.406719247264253, 4.6443908991413725, 4.8283137373023015, 5.707110264748875, 4.007333185232471])
x_theta_3d, y_theta_3d, z_theta_3d = compute_stereo_3d(ratio0_theta, ratio1_theta, ln_cac1_theta)

# Eta data (higher CAC progression)
ratio0_eta = np.array([0.8688607994125579, 1.0630901185378252, 0.9961730542492246, 0.8175811528613812, 0.9622938095066099, 0.9631345449908901, 1.0484241406252823, 0.7062302027241281, 0.9822425957367792, 0.9072391782794026, 0.8193105055662985, 1.1930588223437388, 0.9334484316946257, 1.1395504941992651, 1.0525373565090204, 1.229792181584599, 0.883830680147293])
ratio1_eta = np.array([0.8408890290104237, 0.8983622221657899, 0.8586308679540858, 0.7681527424747342, 0.9330379097641369, 0.8989095019787317, 1.033377601382106, 0.664608080850563, 0.9191112554962112, 0.8835428801490052, 0.7832104228023535, 1.130087818908135, 0.8919864268699415, 1.0594671601894434, 0.9933697952812031, 1.1339433933635201, 0.842936576767417])
ln_cac1_eta = np.array([3.7376696182833684, 4.663439094112067, 4.584967478670572, 4.6443908991413725, 4.584967478670572, 5.389071729816501, 5.5053315359323625, 3.58351893845611, 5.537334267018537, 5.541263545158426, 4.61512051684126, 5.993961427306569, 5.442417710521793, 6.645090969505644, 5.777652323222656, 5.60947179518496, 5.54907608489522])
x_eta_3d, y_eta_3d, z_eta_3d = compute_stereo_3d(ratio0_eta, ratio1_eta, ln_cac1_eta)

# Setup figure and axis
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(111, projection='3d')

# Plot Zeta (yellow, regressors)
zeta_scatter = ax.scatter(x_zeta_3d, y_zeta_3d, z_zeta_3d, c='orange', marker='D', label='Zeta (Regressors)')

# Plot Theta (purple, low/zero CAC increase)
theta_scatter = ax.scatter(x_theta_3d, y_theta_3d, z_theta_3d, c='purple', marker='o', label='Theta (Low/Zero CAC)')

# Plot Eta (green, higher CAC progression)
eta_scatter = ax.scatter(x_eta_3d, y_eta_3d, z_eta_3d, c='green', marker='s', label='Eta (High CAC)')

# Highlight user point in Theta
user_x = 0.772404
user_index_theta = np.argmin(np.abs(ratio0_theta - user_x))
user_scatter = ax.scatter(x_theta_3d[user_index_theta], y_theta_3d[user_index_theta], z_theta_3d[user_index_theta], 
                         c='magenta', marker='*', s=150, label='Your Point')

# Add vector arrows for progression (Theta as example)
mask_theta = (z_theta_3d != 0) & (ratio0_theta != 0) & (ratio1_theta != 0)
for i in np.where(mask_theta)[0]:
    dx = (ratio1_theta[i] - ratio0_theta[i]) * 0.5
    dy = 0
    dz = (ratio1_theta[i] - ratio0_theta[i]) * 40 * 0.5
    ax.quiver(x_theta_3d[i], y_theta_3d[i], z_theta_3d[i], 
              dx, dy, dz, color='purple', alpha=0.5, arrow_length_ratio=0.1)

# Add regression stats
stats_text = (
    "Ln(CAC₀/NCPV₀) vs Ln(CAC₁)\n"
    "Slope: 4.4436 | R²: 0.8359 | p: 0.0315\n"
    "Ln(CAC₀/NCPV₁) vs Ln(CAC₁)\n"
    "Slope: 4.7533 | R²: 0.8502 | p: 0.0235"
)
ax.text2D(0.05, 0.95, stats_text, transform=ax.transAxes, fontsize=8, 
          verticalalignment='top', bbox=dict(facecolor='white', alpha=0.8))

# Set axis labels and title
ax.set_xlabel('Average Ratio (CAC/NCPV)')
ax.set_ylabel('ln(CAC1 + 1)')
ax.set_zlabel('Time Displacement (Disparity x40)')
ax.set_title('3D Fly-Through: Plaque Progression (Time-Shifted)')

# Set fixed axis limits
ax.set_xlim(min(x_zeta_3d.min(), x_theta_3d.min(), x_eta_3d.min()) - 0.5, 
            max(x_zeta_3d.max(), x_theta_3d.max(), x_eta_3d.max()) + 0.5)
ax.set_ylim(min(y_zeta_3d.min(), y_theta_3d.min(), y_eta_3d.min()) - 0.5, 
            max(y_zeta_3d.max(), y_theta_3d.max(), y_eta_3d.max()) + 0.5)
ax.set_zlim(min(z_zeta_3d.min(), z_theta_3d.min(), z_eta_3d.min()) - 5, 
            max(z_zeta_3d.max(), z_theta_3d.max(), x_eta_3d.max()) + 5)

# Add grid
ax.grid(True)

# Add regression stats
stats_text = (
    "Ln(CAC₀/NCPV₀) vs Ln(CAC₁)\n"
    "Slope: 4.4436 | R²: 0.8359 | p: 0.0315\n\n"
    "Ln(CAC₀/NCPV₁) vs Ln(CAC₁)\n"
    "Slope: 4.7533 | R²: 0.8502 | p: 0.0235"
)

ax.text2D(0.05, 0.95, stats_text, transform=ax.transAxes, fontsize=8, 
          verticalalignment='top', bbox=dict(facecolor='white', alpha=0.8))

# Fly-through animation (non-stereo for stability)
def update_view(i):
    azim = i % 360
    elev = 20 + np.sin(i / 180 * np.pi) * 10
    zoom = 10 - 2 * np.sin(i / 180 * np.pi)
    z_shift = -5 + 10 * (i / 360)
    
    # Update user point size
    pulse_size = 150 + 50 * np.sin(i / 90 * np.pi)
    user_scatter._sizes = [pulse_size]
    
    # Update view
    ax.view_init(elev=elev, azim=azim)
    ax.dist = zoom
    ax.set_zlim(min(z_zeta_3d.min(), z_theta_3d.min(), z_eta_3d.min()) - 5 + z_shift, 
                max(z_zeta_3d.max(), z_theta_3d.max(), z_eta_3d.max()) + 5 + z_shift)
    
    return [zeta_scatter, theta_scatter, eta_scatter, user_scatter, ax.texts[0]]

# Create animation
ani = animation.FuncAnimation(fig, update_view, frames=360, interval=33, blit=False)

# Save as GIF
ani.save('fly_through_nonstereo.gif', writer='pillow', fps=30, dpi=100)

print("Fly-through GIF created: 'fly_through_nonstereo.gif'")
plt.close(fig)