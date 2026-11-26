const titlesEl = document.getElementById("titles");
const statusEl = document.getElementById("status");
const excludedEl = document.getElementById("excluded");
const tableBody = document.querySelector("#dusts tbody");
const runButton = document.getElementById("run");
const coolButton = document.getElementById("cool");
const cap = 2000;
const p05 = -Math.log10(0.05);
const p20 = -Math.log10(0.2);

let histChart;
let curveChart;
let rows = [];
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
    const plotted = (body.curve || []).length;
    statusEl.textContent = `${body.titles} titles → ${rows.length} dusts in ${(body.elapsedMs / 1000).toFixed(1)} s.`;
    excludedEl.textContent = `On the chart: ${plotted}. Not plotted: p = 0 (${body.perfect}), invalid p (${body.invalid}), −log10(p) ≥ 10 (${body.overflow}).`;
    drawHistogram(body.bins || []);
    drawCurve(body.curve || []);
    sortKey = "negLog10";
    sortDir = -1;
    paintTable();
    setBusy(false);
}

function drawHistogram(bins) {
    const labels = bins.map((_, i) => (i % 5 === 0 ? (i * 0.2).toFixed(0) : ""));
    histChart = replaceChart(histChart, document.getElementById("hist"), {
        type: "bar",
        data: {
            labels,
            datasets: [{
                data: bins,
                backgroundColor: "#38bdf8",
                borderWidth: 0,
                categoryPercentage: 1,
                barPercentage: 0.95
            }]
        },
        options: chartOptions("−log10(p)", "Dusts"),
        plugins: [verticalRefs]
    });
}

function drawCurve(scores) {
    const n = Math.max(scores.length, 1);
    curveChart = replaceChart(curveChart, document.getElementById("curve"), {
        type: "line",
        data: {
            datasets: [
                {
                    label: "−log10(p)",
                    data: scores.map((y, i) => ({ x: i + 1, y })),
                    borderColor: "#7dd3fc",
                    backgroundColor: "#7dd3fc",
                    pointRadius: scores.length > 80 ? 0 : 3,
                    borderWidth: 2,
                    tension: 0
                },
                referenceLine("p = 0.05", p05, n, "#f43f5e"),
                referenceLine("p = 0.2", p20, n, "#fb923c")
            ]
        },
        options: {
            ...chartOptions("Rank, most significant first", "−log10(p)"),
            plugins: { legend: { labels: { color: "#e5e7eb" } } },
            scales: linearScales("Rank, most significant first", "−log10(p)")
        }
    });
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
    const sorted = [...rows].sort((a, b) => compare(a, b, sortKey) * sortDir);
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
