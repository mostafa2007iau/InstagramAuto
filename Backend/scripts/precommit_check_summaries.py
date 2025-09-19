"""
Persian:
    اسکریپت pre-commit برای بررسی حضور فارسی و انگلیسی در header هر فایل پایتون.

English:
    Pre-commit script to ensure each python file contains Persian and English summary blocks.
"""

import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(__file__))
pattern_en = re.compile(r"English:\s*", re.IGNORECASE)
pattern_fa = re.compile(r"Persian:\s*", re.IGNORECASE)

errors = []
for subdir, dirs, files in os.walk(os.path.join(ROOT, "app")):
    for f in files:
        if f.endswith(".py"):
            path = os.path.join(subdir, f)
            with open(path, "r", encoding="utf-8") as fh:
                sample = fh.read(2000)
                if not pattern_en.search(sample) or not pattern_fa.search(sample):
                    errors.append(path)

if errors:
    print("The following files are missing Persian or English headers:")
    for e in errors:
        print(" -", e)
    sys.exit(2)

print("All files contain Persian and English headers.")
