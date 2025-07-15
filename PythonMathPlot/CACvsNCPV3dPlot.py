import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.patches import Patch

# Zeta data (masked to non-zero)
x_zeta = np.array([0.9710033899612511, 0.9015366321328295, 1.1394427589104996, 1.2745596383334394, 1.0980360579681494, 1.1997997113370333])
y_zeta = np.array([0.9253378102738982, 0.8506729246131384, 1.100342686869395, 1.3085760744476183, 1.0311375010237032, 1.1388484961156078])
z_zeta = np.array([4.143134726391533, 3.6888794541139363, 5.869296913133774, 4.912654885736052, 5.863631175598097, 5.267858159063328])

# Theta data (masked to non-zero)
x_theta = np.array([0.35285263872984957, 0.9647593924678355, 0.877286031413336, 1.0411460407002446, 0.3255009363788492, 0.7724007307085876, 0.6036728262897569, 0.27144150822594215, 0.45205229008535625, 0.8318813233527058, 0.22164943200951182, 0.6604399736330077, 0.8607555653155671, 1.0010636260309163, 0.9175571828446315, 1.0281475831090183, 0.8289472102329216])
y_theta = np.array([0.3297159881728276, 0.8691756693182583, 0.8267776654084305, 0.8923483493991499, 0.31927963388710684, 0.6707750237222871, 0.5776613396057578, 0.25680855464584623, 0.4491694490897574, 0.8033125469454092, 0.20896115182777275, 0.6485015093091424, 0.7963474917371539, 0.8893916599278506, 0.891574050402815, 0.9861109838733201, 0.7557726453415707])
z_theta = np.array([1.9459101490553132, 3.6375861597263857, 2.6390573296152584, 4.219507705176107, 2.1972245773362196, 3.091042453358316, 2.9444389791664403, 1.791759469228055, 2.8903717578961645, 3.912023005428146, 2.0794415416798357, 3.4657359027997265, 4.406719247264253, 4.6443908991413725, 4.8283137373023015, 5.707110264748875, 4.007333185232471])

# Eta data (all non-zero)
x_eta = np.array([0.8688607994125579, 1.0630901185378252, 0.9961730542492246, 0.8175811528613812, 0.9622938095066099, 0.9631345449908901, 1.0484241406252823, 0.7062302027241281, 0.9822425957367792, 0.9072391782794026, 0.8193105055662985, 1.1930588223437388, 0.9334484316946257, 1.1395504941992651, 1.0525373565090204, 1.229792181584599, 0.883830680147293])
y_eta = np.array([0.8408890290104237, 0.8983622221657899, 0.8586308679540858, 0.7681527424747342, 0.9330379097641369, 0.8989095019787317, 1.033377601382106, 0.664608080850563, 0.9191112554962112, 0.8835428801490052, 0.7832104228023535, 1.130087818908135, 0.8919864268699415, 1.0594671601894434, 0.9933697952812031, 1.1339433933635201, 0.842936576767417])
z_eta = np.array([3.7376696182833684, 4.663439094112067, 4.584967478670572, 4.6443908991413725, 4.584967478670572, 5.389071729816501, 5.5053315359323625, 3.58351893845611, 5.537334267018537, 5.541263545158426, 4.61512051684126, 5.993961427306569, 5.442417710521793, 6.645090969505644, 5.777652323222656, 5.60947179518496, 5.54907608489522])

# Toggles
show_planes = False  # Set to False to hide all regression planes

# Create Plot
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(111, projection='3d')

# Plot Zeta (orange diamonds)
ax.scatter(x_zeta, y_zeta, z_zeta, c='orange', marker='D', s=70, label='ζ (Reversing, N=6)')

# Plot Theta (purple circles)
ax.scatter(x_theta, y_theta, z_theta, c='purple', marker='o', label='θ (Smaller CAC increase, N=23)')

# Plot Eta (green squares)
ax.scatter(x_eta, y_eta, z_eta, c='green', marker='s', label='η (Larger CAC increase, N=17)')

# Highlight user point in magenta (in Theta)
user_x = 0.772404
user_index_theta = np.argmin(np.abs(x_theta - user_x))
ax.scatter(x_theta[user_index_theta], y_theta[user_index_theta], z_theta[user_index_theta], c='magenta', marker='*', s=150, label='Your Point')

# Define meshgrid over full range
min_x = min(np.min(x_zeta), np.min(x_theta), np.min(x_eta))
max_x = max(np.max(x_zeta), np.max(x_theta), np.max(x_eta))
min_y = min(np.min(y_zeta), np.min(y_theta), np.min(y_eta))
max_y = max(np.max(y_zeta), np.max(y_theta), np.max(y_eta))
xx, yy = np.meshgrid(np.linspace(min_x, max_x, 10), np.linspace(min_y, max_y, 10))

if show_planes:
    # Zeta regression plane (orange)
    zz_zeta = -2.624 + 21.614 * xx - 15.236 * yy
    ax.plot_surface(xx, yy, zz_zeta, alpha=0.3, color='orange')

    # Theta regression plane (purple)
    zz_theta = 0.788 - 3.937 * xx + 8.262 * yy
    ax.plot_surface(xx, yy, zz_theta, alpha=0.3, color='purple')

    # Eta regression plane (green)
    zz_eta = 0.790 - 1.771 * xx + 6.652 * yy
    ax.plot_surface(xx, yy, zz_eta, alpha=0.3, color='green')

# Add proxies for planes to legend only if shown
plane_proxies = []
plane_labels = []
if show_planes:
    plane_proxies += [
        Patch(color='orange', alpha=0.3, label='ζ Plane'),
        Patch(color='purple', alpha=0.3, label='θ Plane'),
        Patch(color='green', alpha=0.3, label='η Plane')
    ]
    plane_labels += ['ζ Plane', 'θ Plane', 'η Plane']

# Get handles and labels from scatters
handles, labels = ax.get_legend_handles_labels()

# Combine with plane proxies
handles += plane_proxies
labels += plane_labels

# Create legend
ax.legend(handles, labels)

ax.set_xlabel('ln(CAC0 + 1) / ln(NCPV0 + 1)')
ax.set_ylabel('ln(CAC0 + 1) / ln(NCPV1 + 1)')
ax.set_zlabel('ln(CAC1 + 1)')
ax.set_title('3D Scatter of CAC and NCPV Ratios: ζ, θ, η\nwith Group-Specific Regression Planes (Toggleable)')

plt.tight_layout()
plt.show()