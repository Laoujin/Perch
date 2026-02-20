#!/usr/bin/env python3
import glob
import json
import math
import sys
import xml.etree.ElementTree as ET

def main():
    if len(sys.argv) != 2 or sys.argv[1] not in ("linux", "windows"):
        print("Usage: python scripts/check-coverage.py <linux|windows>")
        sys.exit(2)

    job = sys.argv[1]

    files = glob.glob("**/coverage.cobertura.xml", recursive=True)
    if not files:
        print("No coverage files found")
        sys.exit(1)

    total_covered = 0
    total_valid = 0
    for path in files:
        root = ET.parse(path).getroot()
        total_covered += int(root.attrib.get("lines-covered", 0))
        total_valid += int(root.attrib.get("lines-valid", 0))

    if total_valid == 0:
        print("No lines found in coverage data")
        sys.exit(1)

    coverage = int(total_covered / total_valid * 1000) / 10
    print(f"Line coverage ({job}): {coverage}% ({total_covered}/{total_valid})")

    with open("coverage-baseline.json") as f:
        baselines = json.load(f)

    baseline = baselines.get(job, 0.0)
    print(f"Baseline: {baseline}%")

    if coverage < baseline:
        print(f"FAIL: coverage {coverage}% is below baseline {baseline}%")
        sys.exit(1)

    if coverage >= baseline + 1.0:
        print(f"Consider updating baseline from {baseline}% to {coverage}%")

    print("OK")


if __name__ == "__main__":
    main()
