#!/bin/sh
cd "$(dirname "$0")"
if command -v xdg-open >/dev/null 2>&1; then
  xdg-open http://127.0.0.1:5176/ >/dev/null 2>&1 &
fi
exec ./Keto-CTA-GoldMine --urls http://127.0.0.1:5176
