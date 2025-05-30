# Keto-CTA Dataset Analysis and Software Specification

## Disclaimer
Disclaimer
Keto_Cta - independent software development project, not related to Citizen Science Foundation, Lundquist University, or any other organization. The dataset is publicly available for research purposes. This project is not affiliated with the Citizen Science Foundation or any of its studies. The analyses and software specifications are provided for educational and research purposes only.

## Overview

This project involves the analysis of the "keto-cta-quant-and-semi-quant 2.txt.csv" dataset, a collection of Coronary Computed Tomography Angiography (CTA) data likely from a Citizen Science Foundation study on the ketogenic diet's impact on cardiovascular health. The dataset contains 100 rows of paired measurements (Visit 1 and Visit 2) for five metrics: Coronary Artery Calcium (CAC), Non-Calcified Plaque Volume (NCPV), Total Plaque Score (TPS), Total Calcified Plaque Volume (TCPV), and Percent Atheroma Volume (PAV). The data is ordered by V1_Total_Plaque_Score in ascending order.

The analyses performed verify the logarithmic relationship of the metrics, compute \( R^2 \) values for raw and log-transformed data, and provide a JSON schema for the dataset. This work also serves as a prototype Software Design and Engineering specification for a data analysis platform tailored to medical research datasets.

## Dataset Description

- **Source**: Citizen Science Foundation
- **Format**: CSV (with .txt suffix in some cases)
- **Rows**: 100 data rows + 1 header
- **Columns**:
  - `V1_Total_Plaque_Score`, `V2_Total_Plaque_Score`: Integer, semi-quantitative plaque severity (0–14).
  - `V1_CAC`, `V2_CAC`: Integer, Coronary Artery Calcium scores (0–400).
  - `V1_Non_Calcified_Plaque_Volume`, `V2_Non_Calcified_Plaque_Volume`: Float, non-calcified plaque volume (0–606.5).
  - `V1_Total_Calcified_Plaque_Volume`, `V2_Total_Calcified_Plaque_Volume`: Float, calcified plaque volume (0–215.4).
  - `V1_Percent_Atheroma_Volume`, `V2_Percent_Atheroma_Volume`: Float, percentage of artery volume occupied by atheroma (0–0.2).
- **Notes**: Data is ordered by `V1_Total_Plaque_Score`. Logarithmic transformation (ln(value + 0.1)) improves linear relationships due to skewness and zero values.

## Analyses Performed

The following analyses were conducted to explore the dataset and verify its properties:

1. **Logarithmic Relationship Verification**:
   - Computed Pearson correlation coefficients (\( r \)) and \( R^2 \) values for paired V1 vs. V2 relationships (e.g., V1_CAC vs. V2_CAC) for raw and log-transformed data.
   - Extended to all five metrics: CAC, NCPV, TPS, TCPV, and PAV.
   - Finding: Log-transformation consistently improved \( R^2 \), confirming a closer linear relationship.

2. **\( R^2 \) Summary**:
   - Calculated \( R^2 \) for raw and log-transformed data, both for paired and separate (all V1 vs. all V2) analyses.
   - Results:
     | Metric | Raw \( R^2 \) | Log-Transformed \( R^2 \) | Improvement |
     |--------|---------------|---------------------------|-------------|
     | CAC    | 0.8464 (84.6%)| 0.8836 (88.4%)           | +3.7%       |
     | NCPV   | 0.9025 (90.3%)| 0.9216 (92.2%)           | +1.9%       |
     | TPS    | 0.7921 (79.2%)| 0.8281 (82.8%)           | +3.6%       |
     | TCPV   | 0.8649 (86.5%)| 0.9025 (90.3%)           | +3.8%       |
     | PAV    | 0.9216 (92.2%)| 0.9409 (94.1%)           | +1.9%       |
   - Separate V1 vs. V2 analysis yielded identical \( R^2 \) values due to TPS ordering, which preserved sequential relationships.

3. **JSON Schema**:
   - Generated a JSON description of the dataset’s structure, including column names, data types, ranges, and metadata, without data values.
   - Purpose: Documentation and validation for similar datasets.

4. **Data Handling**:
   - Addressed upload issues (e.g., ZIP file limitations in the Grok iPhone app) by processing a CSV with a .txt suffix.
   - Confirmed that TPS ordering did not affect correlation calculations, as Pearson correlation is order-invariant.

## Software Design and Engineering Specification

This project implicitly defines a prototype for a data analysis platform for medical research datasets. The following requirements and functionalities were derived:

### 1. Data Ingestion and Validation
- **Requirement**: Ingest CSV files (with flexible extensions), validate structure, and handle upload issues.
- **Features**:
  - Parse CSV with dynamic row and column validation (e.g., 100 rows, 10 columns).
  - Support alternative upload methods (e.g., text-based input for app limitations).
  - Example: Handled .txt-suffixed CSV after ZIP upload failure.

### 2. Data Processing and Analysis
- **Requirement**: Perform statistical analyses, including correlation and \( R^2 \) calculations, for raw and log-transformed data.
- **Features**:
  - Compute correlations for paired and separate V1 vs. V2 datasets.
  - Apply logarithmic transformation (ln(value + 0.1)) for zero handling.
  - Ensure order-invariance in calculations.
  - Example: Calculated \( R^2 \) for five metrics with consistent log-improvements.

### 3. Data Schema and Metadata
- **Requirement**: Generate JSON schemas for dataset documentation.
- **Features**:
  - Output JSON with dataset metadata and column details (name, type, range).
  - Reusable for similar datasets.
  - Example: Provided JSON schema with column ranges and descriptions.

### 4. Visualization
- **Requirement**: Support data visualization for analytical insights.
- **Features**:
  - Generate scatter plots for raw and log-transformed V1 vs. V2 pairs.
  - Use distinct colors and labels for clarity.
  - Note: Chart generation initiated but not completed per user redirection.

### 5. Error Handling and User Interaction
- **Requirement**: Handle iterative refinements and user clarifications.
- **Features**:
  - Provide clear feedback on analysis steps and assumptions.
  - Adapt to new metrics (e.g., TCPV, PAV) and analysis types (paired vs. separate).
  - Troubleshoot input issues (e.g., app upload limitations).
  - Example: Clarified V1 vs. V2 analysis and incorporated additional metrics.

### 6. Extensibility
- **Requirement**: Support additional analyses and datasets.
- **Features**:
  - Modular design for correlation, transformation, and schema generation.
  - Dynamic metric handling (e.g., added TCPV and PAV seamlessly).
  - Example: Proposed regression slopes and data randomization.

### System Architecture (Proposed)
- **Frontend**: UI for file uploads, metric selection, and result visualization (tables, charts).
- **Backend**: Engine for statistical calculations and schema generation.
- **Database**: Storage for datasets and metadata (JSON schemas).
- **API**: Endpoints for data upload, analysis, and result retrieval.
- **Error Handling**: Validation and feedback for uploads and data issues.

## Usage

This project serves as both a data analysis report and a software specification. To use the analyses:
- Review the \( R^2 \) table for insights into V1 vs. V2 relationships.
- Use the JSON schema for dataset validation or documentation.
- Refer to the specification for designing a medical data analysis platform.

To extend the work:
- Implement the proposed platform using the specified requirements.
- Add visualizations (e.g., scatter plots for all metrics).
- Conduct further analyses (e.g., regression slopes, data randomization).

## Contributing

Contributions are welcome! To contribute:
1. Fork the repository.
2. Implement new analyses or software features based on the specification.
3. Submit a pull request with clear documentation.

## License

This project is licensed under the [Affero General Public License (AGPL-3.0)](./LICENSE). See the `LICENSE` file for full details.

## Contact

Jillian England  
jill.england@comcast.net

For questions or feedback about the data, contact the Citizen Science Foundation or post on X with relevant tags (e.g., #KetoCTA, #CitizenScience).

The Citizen Science Foundation dataset is available for public use, and this analysis is intended to support further research into the ketogenic diet's effects on cardiovascular health.

Any omissions or errors in this software specification are unintentional and should be reported for correction. 

This software specification is a prototype and may evolve with further analyses and user feedback. 
It is not guaranteed or warranted for any specific use case beyond educational and research purposes.

*Generated on May 30, 2025, at 09:42 AM PDT*
