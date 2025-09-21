"""
Persian:
    اسکریپت تولید OpenAPI از اپ زنده. اجرا کن وقتی uvicorn در حال اجراست تا openapi.json/v1 را تولید کند.
    مثال اجرا: python openapi_gen.py http://localhost:8000 openapi_v1.json

English:
    Script to dump OpenAPI from a running FastAPI app. Run while uvicorn is running to produce openapi.json/v1.
    Example: python openapi_gen.py http://localhost:8000 openapi_v1.json
"""

import sys
import requests

def dump_openapi(base_url: str, out_file: str):
    url = base_url.rstrip("/") + "/openapi.json"
    r = requests.get(url)
    r.raise_for_status()
    with open(out_file, "w", encoding="utf-8") as f:
        f.write(r.text)
    print("Saved OpenAPI to", out_file)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python openapi_gen.py BASE_URL OUT_FILE")
        print("Example: python openapi_gen.py http://localhost:8000 openapi_v1.json")
        sys.exit(1)
    dump_openapi(sys.argv[1], sys.argv[2])
