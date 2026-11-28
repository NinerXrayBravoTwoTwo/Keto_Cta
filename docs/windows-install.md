# Windows install

This is the folder build of the GoldMine web UI, for a machine that does not already have the .NET SDK. It is a self-contained `win-x64` publish: the unzipped folder includes the runtime, `wwwroot`, and the two data CSVs. You do not install .NET separately. A framework-dependent build is smaller, but only if .NET 8 is already installed, so the zip uses the self-contained folder instead of a single file. A single file is awkward here because the pages and the CSVs have to sit next to the program.

The software license is AGPL-3.0 (`LICENSE`). The CSVs in the zip are the copies already in `KetoCtaRegressions/TestData`. The README says that dataset is publicly available for research from the Citizen Science Foundation. It does not state a separate license for redistributing those files inside a GitHub Release. Treat that as unanswered until it is confirmed.

## From a draft release

A tag named `v*` (for example `v1.0.0`) starts `.github/workflows/release.yml`. The workflow tests, publishes, and attaches zip files to a **draft** release. Nothing is published until that draft is edited and released by hand. Do not expect a download link before that.

When a Windows zip is available:

1. Download `Keto-CTA-GoldMine-win-x64.zip` and unzip it.
2. Double-click `start-goldmine.bat`.
3. The script opens http://127.0.0.1:5176/ and leaves a console window running. Close that window to stop the server.

Windows may ask for firewall permission. Local use does not need a public rule. If port 5176 is taken, edit the `.bat` and change both `5176` numbers to another port.

## Build the folder yourself

From the repository root, with the .NET 8 SDK:

```powershell
dotnet publish Keto-CTA-GoldMine\Keto-CTA-GoldMine.csproj -c Release -r win-x64 --self-contained -o publish\win-x64
copy Keto-CTA-GoldMine\packaging\start-goldmine.bat publish\win-x64\
```

Then double-click `publish\win-x64\start-goldmine.bat`.

The program reads `wwwroot` and `Data\` from the folder that contains the `.exe`, so the working directory does not matter. The pages are http://127.0.0.1:5176/ and http://127.0.0.1:5176/scatter.html.
