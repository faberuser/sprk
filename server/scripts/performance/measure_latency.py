#!/usr/bin/env python3
"""Compare warmed HTTP connections through LAN and public origins without logging credentials."""
import argparse
import gzip
import http.client
import json
import math
from pathlib import Path
import re
import statistics
import time
from urllib.parse import urlsplit


def percentile(values, fraction):
    return sorted(values)[max(0, math.ceil(len(values) * fraction) - 1)]


def measure(origin, path, count, warmup, body, compact, sections=False):
    address = urlsplit(origin)
    if address.scheme not in ("http", "https") or not address.hostname or address.username or address.password:
        raise ValueError("Use an http(s) origin without credentials")
    if address.path not in ("", "/") or address.query or address.fragment:
        raise ValueError("Specify a bare origin; put the endpoint in --path")
    connection_type = http.client.HTTPSConnection if address.scheme == "https" else http.client.HTTPConnection
    connection = connection_type(address.hostname, address.port, timeout=15)
    headers = {"Content-Type": "application/x-www-form-urlencoded", "Accept-Encoding": "gzip"}
    if compact:
        headers["X-SPRK-Compact-Lobby"] = "1"
    if sections:
        headers["X-SPRK-Sections"] = "1"
    samples = []
    try:
        for index in range(warmup + count):
            started = time.perf_counter()
            connection.request("POST", path, body=body, headers=headers)
            response = connection.getresponse()
            ttfb = (time.perf_counter() - started) * 1000
            data = response.read()
            elapsed = (time.perf_counter() - started) * 1000
            if response.status != 200:
                raise ValueError(f"Endpoint returned HTTP {response.status}")
            value = json.loads(gzip.decompress(data) if response.getheader("Content-Encoding") == "gzip" else data)
            if value.get("BaseResult") not in (1, "Success") or value.get("Result", "Success") != "Success":
                raise ValueError("Endpoint returned an API error; check the form/session")
            for field, header in [("SprkShopCatalogToken", "X-SPRK-Shop-Catalog"), ("SprkProgressionToken", "X-SPRK-Progression"),
                                  ("SprkAchievementToken", "X-SPRK-Achievements"), ("SprkMainQuestToken", "X-SPRK-Main-Quest"),
                                  ("SprkClearMissionToken", "X-SPRK-Clear-Missions"), ("SprkPlayRecordToken", "X-SPRK-Play-Records")]:
                if field in value:
                    headers[header] = value[field]
            timing = re.search(r"(?:^|,)\s*app;dur=([0-9.]+)", response.getheader("Server-Timing", ""))
            if index >= warmup:
                samples.append({"ttfb_ms": ttfb, "total_ms": elapsed, "wire_body_bytes": len(data),
                                "app_ms": float(timing.group(1)) if timing else None,
                                "content_encoding": response.getheader("Content-Encoding", "identity")})
    finally:
        connection.close()
    app_times = [sample["app_ms"] for sample in samples if sample["app_ms"] is not None]
    return {"origin": origin, "path": path, "count": count,
            "ttfb_p50_ms": statistics.median(s["ttfb_ms"] for s in samples),
            "total_p50_ms": statistics.median(s["total_ms"] for s in samples),
            "total_p95_ms": percentile([s["total_ms"] for s in samples], .95),
            "app_p50_ms": statistics.median(app_times) if app_times else None,
            "body_bytes_p50": statistics.median(s["wire_body_bytes"] for s in samples), "samples": samples}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--origin", action="append", required=True, help="Repeat for LAN and public origins")
    parser.add_argument("--path", default="/ping", help="POST endpoint; choose a read-only endpoint")
    parser.add_argument("--count", type=int, default=30)
    parser.add_argument("--warmup", type=int, default=3)
    parser.add_argument("--form-file", type=Path, help="Optional URL-encoded form; contents are never printed")
    parser.add_argument("--compact-lobby", action="store_true")
    parser.add_argument("--sections", action="store_true", help="Negotiate independent progression section acknowledgements")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    if args.count < 1 or args.warmup < 0 or not args.path.startswith("/") or urlsplit(args.path).query or urlsplit(args.path).fragment:
        parser.error("Use count >= 1, warmup >= 0 and an absolute endpoint path")
    body = args.form_file.read_bytes() if args.form_file else b""
    results = [measure(origin, args.path, args.count, args.warmup, body, args.compact_lobby, args.sections) for origin in args.origin]
    payload = json.dumps(results, indent=2)
    if args.output:
        args.output.write_text(payload + "\n", encoding="utf-8")
    for result in results:
        print(f"{result['origin']}: total p50={result['total_p50_ms']:.1f}ms p95={result['total_p95_ms']:.1f}ms; "
              f"app p50={result['app_p50_ms']!r}ms; median body={result['body_bytes_p50']} bytes")


if __name__ == "__main__":
    main()
