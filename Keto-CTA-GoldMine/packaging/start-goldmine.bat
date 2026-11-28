@echo off
cd /d "%~dp0"
start "" http://127.0.0.1:5176/
Keto-CTA-GoldMine.exe --urls http://127.0.0.1:5176
