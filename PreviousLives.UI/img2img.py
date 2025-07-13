#!/usr/bin/env python3
import argparse
import base64
import sqlite3
import sys
import time
import json
from pathlib import Path

import requests
import openai

# ─── FORCE UTF-8 ON STDOUT ─────────────────────────────────────────────────────
# On Windows the default console encoding is often cp1252, which can't
# handle characters like U+2705.  This reconfigures stdout to use UTF-8.
if sys.platform.startswith("win"):
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except AttributeError:
        # older Python 3.6–3.7 may not have reconfigure; ignore in that case
        pass

# ─── CONFIG ────────────────────────────────────────────────────────────────────
# Hard-code your keys here for testing ONLY!
# Hard-code your keys here for testing ONLY!
OPENAI_API_KEY     = "YOUR_API_KEY_HERE"
STABILITY_API_KEY  = "YOUR_API_KEY_HERE"


openai.api_key = OPENAI_API_KEY

# ─── DESCRIPTION VIA GPT ──────────────────────────────────────────────────────
def gen_description(profession: str, age: int) -> str:
    prompt = (
        f"Gender: male\n"
        f"Profession: {profession}\n"
        f"Age at death: {age}\n"
        "Describe what your past life was like, and finish with an EPIC death scene."
    )
    resp = openai.ChatCompletion.create(
        model="gpt-3.5-turbo",
        messages=[{"role": "user", "content": prompt}]
    )
    return resp.choices[0].message["content"].strip()

# ─── CALL STABILITY IMG2IMG ────────────────────────────────────────────────────
def call_img2img(init_image_path: str, profession: str, age: int) -> bytes:
    headers = {
        "Authorization": f"Bearer {STABILITY_API_KEY}",
        "Accept":        "application/json",
    }
    files = {"image": open(init_image_path, "rb")}
    data = {
        "prompt":    f"Dramatic past-life scene of a {profession}, age {age}, high quality, job side background, working the job,8k, cinemtic, clear image, photorealistic",
        "negative_prompt":  "blurry, deformed, low resolution, disfigured, cartoon, abstract, bad quality",
        "mode":             "image-to-image",
        "strength":         "0.5",
        "sampler":          "k_lms",
        "model":            "sd3.5-large-turbo",   
        "steps":            "30",
        "cfg_scale":        "4",
        "style_preset":     "photographic"
    }
    url = "https://api.stability.ai/v2beta/stable-image/generate/sd3"
    r = requests.post(url, headers=headers, files=files, data=data)
    r.raise_for_status()

    j = r.json()
    # dump to inspect
    print("\n\n=== Stability RAW JSON ===", flush=True)
    print(json.dumps(j, indent=2), flush=True)
    print("=== End JSON ===\n\n", flush=True)

    # try the old key first
    if "artifacts" in j:
        b64 = j["artifacts"][0].get("base64")
    elif "images" in j:
        b64 = j["images"][0].get("data") or j["images"][0].get("base64")
    elif "image" in j:
        # new fallback: Stability sometimes returns a top-level "image"
        b64 = j["image"]
    else:
        raise RuntimeError(f"Couldn't find any image key in response JSON. Keys were: {list(j.keys())}")

    if not b64:
        raise RuntimeError(f"Found key but it was empty. Full JSON artifact: {j}")

    return base64.b64decode(b64)

# ─── SAVE TO DB ────────────────────────────────────────────────────────────────
def save_to_db(db_path: str, orig: bytes, edited: bytes, desc: str) -> int:
    conn = sqlite3.connect(db_path)
    c    = conn.cursor()
    c.execute("""
        INSERT INTO Captures (Timestamp, ImageData, Description, EditedImage)
        VALUES (?, ?, ?, ?)
    """, (int(time.time()), orig, desc, edited))
    new_id = c.lastrowid
    conn.commit()
    conn.close()
    return new_id

# ─── MAIN ─────────────────────────────────────────────────────────────────────
def main():
    p = argparse.ArgumentParser("img2img → Stability → DB")
    p.add_argument("--image",      required=True)
    p.add_argument("--profession", required=True)
    p.add_argument("--age",        type=int, required=True)
    p.add_argument("--db",         required=True)
    args = p.parse_args()

    image_path = Path(args.image)
    if not image_path.exists():
        print("ERROR: image not found:", image_path, file=sys.stderr)
        sys.exit(1)

    orig   = image_path.read_bytes()
    desc   = gen_description(args.profession, args.age)
    edited = call_img2img(str(image_path), args.profession, args.age)
    new_id = save_to_db(args.db, orig, edited, desc)
    # removed the checkmark emoji to avoid Windows CP1252 issues
    print(f"Saved capture #{new_id}")

if __name__ == "__main__":
    main()

