const sets = ["Omega", "Alpha", "Beta", "Zeta", "Gamma", "Theta", "Eta", "BetaUZeta"];
const colors = ["#ef4444", "#3b82f6"];
const legendPlaces = {
    tl: { corner: "tl", position: "top", align: "center" },
    tr: { corner: "tr", position: "top", align: "end" },
    bl: { corner: "bl", position: "bottom", align: "start" },
    br: { corner: "br", position: "bottom", align: "end" }
};

const titleEl = document.getElementById("title");
const setEl = document.getElementById("set");
const title2El = document.getElementById("title2");
const set2El = document.getElementById("set2");
const extraEl = document.getElementById("extra");
const messageEl = document.getElementById("message");
const themeButton = document.getElementById("theme");
const pngButton = document.getElementById("png");
const csvButton = document.getElementById("csv");
const canvas = document.getElementById("scatter");
const legendEl = document.getElementById("legend");

let chart;
let drawn = [];
let levelParam = new URLSearchParams(location.search).get("level");

fillSelect(setEl, "Beta");
fillSelect(set2El, "Zeta");
syncThemeButton();

document.getElementById("form").addEventListener("submit", event => {
    event.preventDefault();
    plot();
});

document.getElementById("add").addEventListener("click", () => {
    extraEl.hidden = false;
    if (!title2El.value.trim()) title2El.value = titleEl.value;
    if (set2El.value === setEl.value) set2El.value = setEl.value === "Zeta" ? "Alpha" : "Zeta";
    if (titleEl.value.trim()) plot();
});

document.getElementById("remove").addEventListener("click", () => {
    extraEl.hidden = true;
    if (titleEl.value.trim()) plot();
});

document.getElementById("meanBand").addEventListener("change", () => { if (drawn.length) draw(drawn); });
document.getElementById("predictionBand").addEventListener("change", () => { if (drawn.length) draw(drawn); });
legendEl.addEventListener("change", () => {
    rememberLegend(legendEl.value);
    if (drawn.length) draw(drawn);
});

themeButton.addEventListener("click", () => {
    const next = document.documentElement.dataset.theme === "light" ? "dark" : "light";
    if (next === "light") document.documentElement.dataset.theme = "light";
    else delete document.documentElement.dataset.theme;
    try { localStorage.setItem("keto-theme", next); } catch (error) { /* private mode */ }
    syncThemeButton();
    if (drawn.length) draw(drawn);
});

pngButton.addEventListener("click", () => {
    if (!chart) return;
    const link = document.createElement("a");
    link.href = chart.toBase64Image("image/png", 1);
    link.download = fileStem() + ".png";
    link.click();
});

csvButton.addEventListener("click", () => {
    const lines = ["id,x,y,set"];
    for (const series of drawn) {
        for (const point of series.points) {
            lines.push([csvField(point.id), point.x, point.y, csvField(series.set)].join(","));
        }
    }
    const blob = new Blob([lines.join("\n") + "\n"], { type: "text/csv" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileStem() + ".csv";
    link.click();
    URL.revokeObjectURL(link.href);
});

legendEl.value = initialLegend();

const params = new URLSearchParams(location.search);
if (params.get("title")) titleEl.value = params.get("title");
if (params.get("set") && sets.includes(params.get("set"))) setEl.value = params.get("set");
if (params.get("title2")) {
    extraEl.hidden = false;
    title2El.value = params.get("title2");
    if (params.get("set2") && sets.includes(params.get("set2"))) set2El.value = params.get("set2");
}
if (titleEl.value.trim()) plot();

function fillSelect(select, selected) {
    select.replaceChildren(...sets.map(name => {
        const option = document.createElement("option");
        option.value = name;
        option.textContent = name;
        option.selected = name === selected;
        return option;
    }));
}

async function plot() {
    messageEl.textContent = "Loading…";
    const wanted = [{ title: titleEl.value.trim(), set: setEl.value }];
    if (!extraEl.hidden && title2El.value.trim())
        wanted.push({ title: title2El.value.trim(), set: set2El.value });
    const series = [];
    const errors = [];
    // One at a time. AuDust caches the selector, and two calls for the same title overlap on that cache.
    for (const item of wanted) {
        try {
            series.push(await loadSeries(item.title, item.set));
        } catch (error) {
            errors.push(error.message);
        }
    }
    if (!series.length) {
        messageEl.textContent = errors[0] || "No series.";
        pngButton.disabled = true;
        csvButton.disabled = true;
        return;
    }
    draw(series);
    const query = new URLSearchParams({ title: series[0].title, set: series[0].set });
    if (series[1]) {
        query.set("title2", series[1].title);
        query.set("set2", series[1].set);
    }
    if (levelParam) query.set("level", levelParam);
    query.set("legend", legendPlace().corner);
    history.replaceState(null, "", "scatter.html?" + query.toString());
    messageEl.textContent = errors.length ? errors.join(" ") : series.map(item => `${item.set}: N=${item.fit.n}`).join(" · ");
}

async function loadSeries(title, set) {
    const query = new URLSearchParams({ title, set });
    if (levelParam) query.set("level", levelParam);
    const response = await fetch("/api/scatter?" + query.toString());
    const body = await response.json();
    if (!response.ok) throw new Error(body.error || body.title || response.statusText);
    return body;
}

function draw(series) {
    drawn = series;
    const theme = currentTheme();
    const datasets = [];
    series.forEach((item, index) => {
        const color = colors[index % colors.length];
        const label = item.title === series[0].title ? item.set : `${item.title} — ${item.set}`;
        datasets.push(...bandDatasets(item, color, label));
        const line = fitLine(item);
        if (line) {
            datasets.push({
                type: "line",
                label: label + " fit",
                data: line,
                borderColor: color,
                borderWidth: 2,
                pointRadius: 0,
                pointHitRadius: 0,
                fill: false,
                order: 1
            });
        }
        datasets.push({
            type: "scatter",
            label,
            data: item.points.map(point => ({ x: point.x, y: point.y, id: point.id })),
            backgroundColor: color,
            borderColor: color,
            pointRadius: 4,
            pointHoverRadius: 6,
            order: 0
        });
    });

    if (chart) chart.destroy();
    chart = new Chart(canvas, {
        type: "scatter",
        data: { datasets },
        options: {
            animation: false,
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: legendPlace().position,
                    align: legendPlace().align,
                    labels: {
                        color: theme.text,
                        filter: item => !/ (fit|band|prediction)$/.test(item.text)
                    }
                },
                title: {
                    display: true,
                    color: theme.text,
                    font: { size: 16 },
                    text: chartTitle(series)
                },
                tooltip: {
                    filter: item => item.raw && item.raw.id != null,
                    callbacks: {
                        label(context) {
                            const point = context.raw;
                            if (!point || point.id == null) return context.dataset.label;
                            return `${point.id}: (${formatNumber(point.x)}, ${formatNumber(point.y)})`;
                        }
                    }
                },
                statsBox: { series, corner: legendPlace().corner }
            },
            scales: {
                x: axis(series[0].xLabel, theme),
                y: axis(series[0].yLabel, theme)
            }
        },
        plugins: [backgroundPlugin, statsPlugin]
    });
    pngButton.disabled = false;
    csvButton.disabled = false;
}

function bandDatasets(item, color, label) {
    const datasets = [];
    if (document.getElementById("predictionBand").checked && item.predictionBand) {
        datasets.push(dashed(label + " prediction", item.predictionBand, "upper", color));
        datasets.push(dashed(label + " prediction", item.predictionBand, "lower", color));
    }
    if (document.getElementById("meanBand").checked && item.meanBand) {
        datasets.push({
            type: "line",
            label: label + " band",
            data: item.meanBand.map(row => ({ x: row.x, y: row.upper })),
            borderWidth: 0,
            pointRadius: 0,
            pointHitRadius: 0,
            fill: "+1",
            backgroundColor: withAlpha(color, 0.2),
            tension: 0,
            order: 3
        });
        datasets.push({
            type: "line",
            label: label + " band",
            data: item.meanBand.map(row => ({ x: row.x, y: row.lower })),
            borderWidth: 0,
            pointRadius: 0,
            pointHitRadius: 0,
            tension: 0,
            order: 3
        });
    }
    return datasets;
}

function dashed(label, band, edge, color) {
    return {
        type: "line",
        label,
        data: band.map(row => ({ x: row.x, y: row[edge] })),
        borderColor: color,
        borderDash: [6, 4],
        borderWidth: 1.5,
        pointRadius: 0,
        pointHitRadius: 0,
        fill: false,
        tension: 0,
        order: 2
    };
}

function withAlpha(hex, alpha) {
    const value = parseInt(hex.slice(1), 16);
    return `rgba(${(value >> 16) & 255}, ${(value >> 8) & 255}, ${value & 255}, ${alpha})`;
}

function formatLevel(level) {
    if (level == null) return "";
    const percent = Math.round(level * 1000) / 10;
    return Number.isInteger(percent) ? `${percent}%` : `${percent.toFixed(1)}%`;
}

function fitLine(series) {
    const fit = series.fit;
    if (fit.slope == null || fit.intercept == null || series.points.length < 2) return null;
    const xs = series.points.map(point => point.x);
    const min = Math.min(...xs);
    const max = Math.max(...xs);
    if (min === max) return null;
    return [
        { x: min, y: fit.slope * min + fit.intercept },
        { x: max, y: fit.slope * max + fit.intercept }
    ];
}

function chartTitle(series) {
    const setsLabel = series.map(item => item.set).join(" / ");
    const sameTitle = series.every(item => item.title === series[0].title);
    const heading = sameTitle ? `${series[0].yLabel} vs. ${series[0].xLabel}` : series.map(item => item.title).join("  ·  ");
    return `${heading} — ${setsLabel}`;
}

function axis(title, theme) {
    return {
        title: { display: true, text: title, color: theme.text },
        ticks: { color: theme.muted },
        grid: { color: theme.grid }
    };
}

const backgroundPlugin = {
    id: "themeBackground",
    beforeDraw(chart) {
        const { ctx, width, height } = chart;
        ctx.save();
        ctx.fillStyle = currentTheme().bg;
        ctx.fillRect(0, 0, width, height);
        ctx.restore();
    }
};

const statsPlugin = {
    id: "statsBox",
    afterDraw(chart) {
        const series = chart.options.plugins.statsBox?.series || [];
        if (!series.length) return;
        const theme = currentTheme();
        const lines = [];
        series.forEach((item, index) => {
            const color = colors[index % colors.length];
            const fit = item.fit;
            const name = item.title === series[0].title ? item.set : `${item.set}: ${item.title}`;
            lines.push({ color, text: name });
            lines.push({ color, text: `${formatLevel(item.level)} band` });
            lines.push({ color, text: `Slope ${formatNumber(fit.slope)}    N = ${fit.n}` });
            lines.push({ color, text: `y-int ${formatNumber(fit.intercept)}    SE ${formatNumber(fit.slopeStandardError)}` });
            lines.push({ color, text: `R² ${formatNumber(fit.rSquared)}` });
            lines.push({ color, text: `p ${formatP(fit.p)}` });
        });
        const ctx = chart.ctx;
        ctx.save();
        ctx.font = "13px Segoe UI, sans-serif";
        const width = Math.max(...lines.map(line => ctx.measureText(line.text).width)) + 20;
        const height = lines.length * 18 + 12;
        const corner = chart.options.plugins.statsBox?.corner || "tl";
        const { left, top } = statsOrigin(chart.chartArea, width, height, corner);
        ctx.fillStyle = theme.box;
        ctx.strokeStyle = theme.grid;
        ctx.lineWidth = 1;
        ctx.beginPath();
        ctx.roundRect(left, top, width, height, 6);
        ctx.fill();
        ctx.stroke();
        lines.forEach((line, index) => {
            ctx.fillStyle = line.color;
            ctx.fillText(line.text, left + 10, top + 18 + index * 18);
        });
        ctx.restore();
    }
};

function legendPlace() {
    return legendPlaces[legendEl.value] || legendPlaces.tl;
}

function initialLegend() {
    const fromUrl = new URLSearchParams(location.search).get("legend");
    if (legendPlaces[fromUrl]) return fromUrl;
    try {
        const stored = localStorage.getItem("keto-legend");
        if (legendPlaces[stored]) return stored;
    } catch (error) { /* private mode */ }
    return "tl";
}

function rememberLegend(value) {
    if (!legendPlaces[value]) return;
    try { localStorage.setItem("keto-legend", value); } catch (error) { /* private mode */ }
    const query = new URLSearchParams(location.search);
    query.set("legend", value);
    const search = query.toString();
    history.replaceState(null, "", search ? "scatter.html?" + search : "scatter.html");
}

function statsOrigin(area, width, height, corner) {
    const pad = 8;
    const right = Math.max(area.left + pad, area.right - width - pad);
    const bottom = Math.max(area.top + pad, area.bottom - height - pad);
    if (corner === "tr") return { left: right, top: area.top + pad };
    if (corner === "bl") return { left: area.left + pad, top: bottom };
    if (corner === "br") return { left: right, top: bottom };
    return { left: area.left + pad, top: area.top + pad };
}

function currentTheme() {
    const styles = getComputedStyle(document.documentElement);
    const light = document.documentElement.dataset.theme === "light";
    return {
        bg: styles.getPropertyValue("--bg").trim(),
        text: styles.getPropertyValue("--text").trim(),
        muted: styles.getPropertyValue("--muted").trim(),
        grid: styles.getPropertyValue("--grid").trim(),
        box: light ? "rgba(255,255,255,0.9)" : "rgba(15,23,42,0.88)"
    };
}

function syncThemeButton() {
    themeButton.textContent = document.documentElement.dataset.theme === "light" ? "Dark theme" : "Light theme";
}

function formatNumber(value) {
    if (value == null || Number.isNaN(value)) return "n/a";
    const abs = Math.abs(value);
    if (abs !== 0 && (abs < 0.0001 || abs >= 10000)) return Number(value).toExponential(3);
    return Number(value).toFixed(4);
}

function formatP(value) {
    if (value == null || Number.isNaN(value)) return "n/a";
    if (value === 0) return "0";
    if (value < 0.001) return Number(value).toExponential(3);
    return Number(value).toFixed(4);
}

function fileStem() {
    return drawn.map(item => `${item.title} -- ${item.set}`).join(" + ").replace(/[\\/:*?"<>|]/g, "");
}

function csvField(value) {
    const text = String(value ?? "");
    return /[",\n]/.test(text) ? `"${text.replaceAll('"', '""')}"` : text;
}
