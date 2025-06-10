# Keto-CTA Dataset Analysis and Software Specification

![Logo](./logo.jpg)

## Disclaimer

This project, **Keto_CTA**, is an independent software development initiative. It is **not affiliated with** the Citizen Science Foundation, Lundquist Institute, or any other organization. The dataset used is publicly available for research from the Citizen Science Foundation. All analyses and software specifications herein are provided solely for educational and research purposes.

---

## Overview

This project analyzes the **keto-cta-quant-and-semi-quant 2.txt.csv** dataset, which includes Coronary Computed Tomography Angiography (CTA) data related to the ketogenic diet and cardiovascular health. The dataset consists of 100 paired measurements (Visit 1 and Visit 2) for five key metrics:

- Coronary Artery Calcium (CAC)
- Non-Calcified Plaque Volume (NCPV)
- Total Plaque Score (TPS)
- Total Calcified Plaque Volume (TCPV)
- Percent Atheroma Volume (PAV)

The data is sorted by `V1_Total_Plaque_Score` in ascending order. This project also functions as a prototype **software design and engineering specification** for a medical research data analysis platform.

---

## Dataset Description

- **Source**: Citizen Science Foundation  
- **Format**: CSV (.txt extension)  
- **Rows**: 100 (plus header)  
- **Columns**:
  - `V1_Total_Plaque_Score`, `V2_Total_Plaque_Score`: Integer (0–14)
  - `V1_CAC`, `V2_CAC`: Integer (0–400)
  - `V1_Non_Calcified_Plaque_Volume`, `V2_Non_Calcified_Plaque_Volume`: Float (0–606.5)
  - `V1_Total_Calcified_Plaque_Volume`, `V2_Total_Calcified_Plaque_Volume`: Float (0–215.4)
  - `V1_Percent_Atheroma_Volume`, `V2_Percent_Atheroma_Volume`: Float (0–0.2)

> **Note**: Logarithmic transformation using `ln(|value| + 1)` improves linear relationships and handles skewed distributions with zero values.


### Omega Set Hierarchy
- This Omega set hierarchy was used to categorize Participants based in the presence of  Plaque reversal and their CAC scores.
![Logo](./Analysis/Keto-CTA-SubsetDivisionTree.png).
-The hierarchy is based on the presence of plaque reversal and CAC scores, with the following categories:
**Definitions based on the provided sets and conditions:**

- **Ω (Omega)** : All participants  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 100 participants
- **α (Alpha)** : { x ∈ Ω | ¬isZeta(x) }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 88 participants (CAC and TPS stable or increasing)
- **ζ (Zeta)** : { x ∈ Ω | isZeta(x) }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 12 participants (CAC or TPS decrease, “Unicorns”)
- **β (Beta)** : { x ∈ α | cac1(x) ≠ 0 ∨ cac2(x) ≠ 0 }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 40 participants (non-zero CAC in α)
- **γ (Gamma)** : { x ∈ α | cac1(x) = 0 ∧ cac2(x) = 0 }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 4 participants (zero CAC in α)
- **η (Eta)** : { x ∈ β | Δcac(x) > 10 }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 17 participants (larger CAC increase)
- **θ (Theta)** : { x ∈ β | Δcac(x) ≤ 10 }  
  &nbsp;&nbsp;&nbsp;&nbsp;◦ 23 participants (smaller CAC increase)

---

## Analyses Performed

### 1. Logarithmic Relationship Verification
- Computed Pearson correlations and \( R^2 \) values between V1 and V2 metrics (both raw and log-transformed).
- Found consistent improvement in linearity after log transformation.

### 2. \( R^2 \) Summary Table (Top Results)

| Index | Metric | Type | Slope | N | R² | P-Value | Y-Intercept |
|-------|--------|------|-------|----|------|----------|-------------|
| 89    | TCPV (raw)       | Omega | 1.128 | 100 | 0.9819 | 0          | 1.6216 |
| 97    | CAC (raw)        | Omega | 1.180 | 100 | 0.9780 | ~0         | -0.555 |
| 137   | CAC (log-log)    | Omega | 1.012 | 100 | 0.9731 | ~0         | 0.0818 |
| 129   | TCPV (log-log)   | Omega | 1.002 | 100 | 0.9675 | 0.00046    | 0.2178 |
| 121   | PAV (log-log)    | Omega | 1.157 | 100 | 0.9498 | 0.000039   | 0.0062 |
| ...   | [*See full table in analysis folder*](./Analysis/Keto-CTA-Regressions.txt).|

> Lower correlations were observed in delta (D) vs. delta comparisons, reflecting greater variability.
### 3. JSON Schema Generation
- Created a metadata schema including column names, types, and value ranges.
- Intended for dataset validation and documentation.

### 4. Data Handling Considerations
- Handled .txt-suffixed CSV files due to mobile app limitations.
- Verified that data ordering by TPS did not affect Pearson correlations.

---

## Software Design and Engineering Specification

The following modules define the prototype platform for dataset ingestion, analysis, and visualization:

### 1. Data Ingestion & Validation
- Parse CSV files with optional `.txt` suffix.
- Validate dimensions (100 rows × 10 columns).
- Support alternative input methods for limited environments (e.g., mobile ZIP uploads).

### 2. Statistical Analysis
- Correlation and \( R^2 \) computation for raw and log-transformed data.
- Ensure invariance to input row order.
- Handle zero values with `ln(value + 0.1)` transform.

### 3. Schema Generation
- JSON output for column metadata (name, type, range).
- Reusable for similar studies.

### 4. Visualization (Planned)
- Scatter plots (raw/log) with labeled axes and trendlines.
- Color-coded metrics for comparative insight.

### 5. Error Handling and UX
- Feedback on missing/invalid inputs.
- Adaptive metrics and analysis expansion.
- Resilience to user/environment constraints.

### 6. Extensibility
- Modular design for new datasets, transformations, and chart types.
- Example: Addition of TCPV and PAV without structural changes.

### Proposed Architecture

- **Frontend**: Upload, metric selection, visual output  
- **Backend**: Statistical engine, schema logic  
- **Database**: Stores datasets and schemas  
- **API**: Upload, analyze, retrieve  
- **UX**: Emphasis on clarity and scientific rigor

---

## Usage

To use this project:
- Explore the summary statistics and correlation results.
- Apply the JSON schema for validation.
- Reference the engineering spec to build or extend analytical tools.

To contribute:
1. Fork the repo.
2. Add new features, analyses, or visualizations.
3. Submit a pull request with a clear explanation.

---

## License

Licensed under the [Affero General Public License (AGPL-3.0)](./LICENSE).

---

## Contact

**Jillian England**  
📧 jill.england@comcast.net  

For dataset inquiries, contact the Citizen Science Foundation or join discussion on social media (e.g., #KetoCTA, #CitizenScience).

> This is a living document. Please report errors or suggest improvements for the benefit of the research community.

---

*Generated: May 30, 2025 at 09:42 AM PDT*  
*Last Edited: May 30, 2025 at 09:43 AM PDT*
