const titlesEl = document.getElementById("titles");
const statusEl = document.getElementById("status");
const excludedEl = document.getElementById("excluded");
const tableBody = document.querySelector("#dusts tbody");
const runButton = document.getElementById("run");
const coolButton = document.getElementById("cool");
const cap = 2000;
const p05 = -Math.log10(0.05);
const p20 = -Math.log10(0.2);

const setColors = {
    Omega: "#e5e7eb",
    Alpha: "#38bdf8",
    Beta: "#a78bfa",
    Zeta: "#f472b6",
    Gamma: "#facc15",
    Theta: "#34d399",
    Eta: "#fb923c",
    BetaUZeta: "#94a3b8"
};

let histChart;
let curveChart;
let rows = [];
let sets = [];
let selected = new Set();
let sortKey = "negLog10";
let sortDir = -1;

coolButton.addEventListener("click", async () => {
    setBusy(true, "Loading matrix cool…");
    try {
        const response = await fetch("/api/presets/cool");
        const body = await response.json();
        if (!response.ok) throw new Error(body.error || body.title || response.statusText);
        titlesEl.value = (body.titles || []).join("\n");
        setBusy(false, `${body.titles.length} matrix cool titles loaded. Press Run.`);
    } catch (error) {
        setBusy(false, error.message);
    }
});

runButton.addEventListener("click", async () => {
    const titles = titlesEl.value.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
    if (titles.length === 0) {
        statusEl.textContent = "Paste at least one title.";
        return;
    }
    if (titles.length > cap) {
        statusEl.textContent = `${titles.length} titles is over the cap of ${cap}. Full mine stays on the console.`;
        return;
    }

    setBusy(true, `Running ${titles.length} title${titles.length === 1 ? "" : "s"}…`);
    try {
        const response = await fetch("/api/runs", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ titles })
        });
        const body = await response.json();
        if (!response.ok) throw new Error(body.error || body.title || response.statusText);
        render(body);
    } catch (error) {
        setBusy(false, error.message);
    }
});

document.querySelectorAll("#dusts th").forEach(header => {
    header.addEventListener("click", () => {
        const key = header.dataset.key;
        sortDir = sortKey === key ? -sortDir : (key === "title" || key === "set" ? 1 : -1);
        sortKey = key;
        paintTable();
    });
});

function render(body) {
    rows = body.dusts || [];
    sets = body.sets || [];
    selected = new Set(sets.map(set => set.name));
    statusEl.textContent = `${body.titles} titles → ${rows.length} dusts in ${(body.elapsedMs / 1000).toFixed(1)} s.`;
    sortKey = "negLog10";
    sortDir = -1;
    paintSets();
    applyFilter();
    setBusy(false);
}

function checkedSets() {
    return sets.filter(set => selected.has(set.name));
}

function applyFilter() {
    const active = checkedSets();
    const perfect = active.reduce((sum, set) => sum + set.perfect, 0);
    const invalid = active.reduce((sum, set) => sum + set.invalid, 0);
    const overflow = active.reduce((sum, set) => sum + set.overflow, 0);
    const plotted = active.reduce((sum, set) => sum + set.curve.length, 0);
    excludedEl.textContent = `Showing ${active.length} of ${sets.length} sets. On the chart: ${plotted}. Not plotted: p = 0 (${perfect}), invalid p (${invalid}), −log10(p) ≥ 10 (${overflow}).`;
    drawHistogram(active);
    drawCurve(active);
    paintTable();
}

function paintSets() {
    const body = document.querySelector("#sets tbody");
    body.replaceChildren(...sets.map(set => {
        const tr = document.createElement("tr");
        const checkCell = document.createElement("td");
        const box = document.createElement("input");
        box.type = "checkbox";
        box.checked = selected.has(set.name);
        box.addEventListener("change", () => {
            if (box.checked) selected.add(set.name);
            else selected.delete(set.name);
            applyFilter();
        });
        checkCell.append(box);

        const name = document.createElement("td");
        const label = document.createElement("span");
        label.className = "set-name";
        const swatch = document.createElement("span");
        swatch.className = "swatch";
        swatch.style.background = colorFor(set.name);
        label.append(swatch, document.createTextNode(set.name));
        name.append(label);

        tr.append(checkCell, name, cell(String(set.dusts)));
        for (const count of set.decades || []) tr.append(cell(String(count)));
        tr.append(cell(String(set.overflow)), cell(String(set.perfect)), cell(String(set.invalid)));
        return tr;
    }));
}

function drawHistogram(active) {
    const width = active[0]?.bins?.length || 50;
    const labels = Array.from({ length: width }, (_, i) => (i % 5 === 0 ? (i * 0.2).toFixed(0) : ""));
    histChart = replaceChart(histChart, document.getElementById("hist"), {
        type: "bar",
        data: {
            labels,
            datasets: active.map(set => ({
                label: set.name,
                data: set.bins,
                backgroundColor: colorFor(set.name),
                borderWidth: 0,
                categoryPercentage: 1,
                barPercentage: 0.95
            }))
        },
        options: {
            ...chartOptions("−log10(p)", "Dusts"),
            plugins: { legend: { labels: { color: "#e5e7eb" } } },
            scales: {
                x: { ...axis("−log10(p)"), stacked: true },
                y: { ...axis("Dusts"), stacked: true, beginAtZero: true, ticks: { color: "#94a3b8", precision: 0 } }
            }
        },
        plugins: [verticalRefs]
    });
}

function drawCurve(active) {
    const ranked = active
        .flatMap(set => set.curve.map(y => ({ name: set.name, y })))
        .sort((a, b) => b.y - a.y)
        .map((point, index) => ({ ...point, x: index + 1 }));
    const n = Math.max(ranked.length, 1);
    curveChart = replaceChart(curveChart, document.getElementById("curve"), {
        type: "scatter",
        data: {
            datasets: [
                ...active.map(set => ({
                    label: set.name,
                    data: ranked.filter(point => point.name === set.name).map(point => ({ x: point.x, y: point.y })),
                    backgroundColor: colorFor(set.name),
                    pointRadius: ranked.length > 120 ? 2 : 4
                })),
                referenceLine("p = 0.05", p05, n, "#f43f5e"),
                referenceLine("p = 0.2", p20, n, "#fb923c")
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { labels: { color: "#e5e7eb" } } },
            scales: linearScales("Rank, most significant first", "−log10(p)")
        }
    });
}

function colorFor(name) {
    return setColors[name] || "#cbd5e1";
}

function referenceLine(label, y, n, color) {
    return {
        label,
        data: [{ x: 1, y }, { x: n, y }],
        borderColor: color,
        borderDash: [6, 4],
        pointRadius: 0,
        borderWidth: 2
    };
}

const verticalRefs = {
    id: "verticalRefs",
    afterDraw(chart) {
        const scale = chart.scales.x;
        if (!scale || chart.data.labels.length < 2) return;
        const step = scale.getPixelForValue(1) - scale.getPixelForValue(0);
        const left = scale.getPixelForValue(0) - step / 2;
        const right = scale.getPixelForValue(chart.data.labels.length - 1) + step / 2;
        for (const line of [{ value: p05, color: "#f43f5e" }, { value: p20, color: "#fb923c" }]) {
            const px = left + (line.value / 10) * (right - left);
            const ctx = chart.ctx;
            ctx.save();
            ctx.strokeStyle = line.color;
            ctx.setLineDash([6, 4]);
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(px, chart.scales.y.top);
            ctx.lineTo(px, chart.scales.y.bottom);
            ctx.stroke();
            ctx.restore();
        }
    }
};

function chartOptions(xTitle, yTitle) {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: categoryScales(xTitle, yTitle)
    };
}

function categoryScales(xTitle, yTitle) {
    return {
        x: axis(xTitle),
        y: { ...axis(yTitle), beginAtZero: true, ticks: { color: "#94a3b8", precision: 0 } }
    };
}

function linearScales(xTitle, yTitle) {
    return {
        x: { ...axis(xTitle), type: "linear" },
        y: { ...axis(yTitle), beginAtZero: true }
    };
}

function axis(title) {
    return {
        title: { display: true, text: title, color: "#cbd5e1" },
        ticks: { color: "#94a3b8" },
        grid: { color: "#243044" }
    };
}

function replaceChart(existing, canvas, config) {
    if (existing) existing.destroy();
    return new Chart(canvas, config);
}

function paintTable() {
    const visible = rows.filter(row => selected.has(row.set));
    const sorted = visible.sort((a, b) => compare(a, b, sortKey) * sortDir);
    tableBody.replaceChildren(...sorted.map(row => {
        const tr = document.createElement("tr");
        tr.append(
            cell(row.title),
            cell(row.set),
            cell(fmt(row.n, 0)),
            cell(fmt(row.slope, 4)),
            cell(fmt(row.rSquared, 4)),
            cell(row.p == null ? "" : Number(row.p).toExponential(3)),
            cell(row.negLog10 == null ? "" : Number(row.negLog10).toFixed(2))
        );
        return tr;
    }));
}

function cell(text) {
    const td = document.createElement("td");
    td.textContent = text ?? "";
    return td;
}

function compare(a, b, key) {
    const av = a[key];
    const bv = b[key];
    if (av == null && bv == null) return 0;
    if (av == null) return 1;
    if (bv == null) return -1;
    return typeof av === "string" ? av.localeCompare(bv) : av - bv;
}

function fmt(value, digits) {
    return value == null ? "" : Number(value).toFixed(digits);
}

function setBusy(busy, message) {
    runButton.disabled = busy;
    coolButton.disabled = busy;
    if (message != null) statusEl.textContent = message;
}
