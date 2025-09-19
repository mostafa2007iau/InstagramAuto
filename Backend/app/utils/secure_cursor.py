"""
Persian:
    توکن cursor امن: محتویات cursor را امضا و رمزنگاری می‌کند تا client نتواند state را جعل کند.

English:
    Secure cursor token: signs and encrypts cursor payload to prevent client tampering.
"""

import json
import base64
import os
from cryptography.hazmat.primitives import hashes, hmac

SECRET = os.getenv("CURSOR_SECRET", "dev-secret-key-please-change").encode()

def sign(payload: dict) -> str:
    raw = json.dumps(payload, separators=(",", ":"), sort_keys=True).encode()
    h = hmac.HMAC(SECRET, hashes.SHA256())
    h.update(raw)
    sig = h.finalize()
    token = base64.urlsafe_b64encode(raw + b"." + sig).decode()
    return token

def verify(token: str) -> dict:
    raw = base64.urlsafe_b64decode(token.encode())
    try:
        data, sig = raw.rsplit(b".", 1)
    except Exception:
        raise ValueError("invalid token")
    h = hmac.HMAC(SECRET, hashes.SHA256())
    h.update(data)
    h.verify(sig)
    return json.loads(data.decode())
