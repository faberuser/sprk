#!/usr/bin/env python3
"""
King's Raid GM CLI
Interact with the private server's cheat endpoints.

Usage (interactive menu):
    python gm.py

Usage (one-shot commands):
    python gm.py session <SESSION_ID>        -- save session ID for future calls
    python gm.py allheroes [--level N] [--star N]
    python gm.py unlock
    python gm.py currency [--gold N] [--gem N] [--stamina N]
    python gm.py hero <HERO_ID> [--level N] [--star N]
    python gm.py level <LEVEL>
    python gm.py reset [--keep-heroes]

Config is saved to gm_config.json next to this script.
"""

import sys
import json
import urllib.request
import urllib.parse
import urllib.error
import argparse
from pathlib import Path

# ---------------------------------------------------------------------------
# Config persistence
# ---------------------------------------------------------------------------
CONFIG_FILE = Path(__file__).parent / "gm_config.json"
DEFAULT_SERVER = "http://127.0.0.1:8080"


def load_config() -> dict:
    if CONFIG_FILE.exists():
        try:
            return json.loads(CONFIG_FILE.read_text())
        except Exception:
            pass
    return {"server": DEFAULT_SERVER, "session_id": ""}


def save_config(cfg: dict):
    CONFIG_FILE.write_text(json.dumps(cfg, indent=2))


# ---------------------------------------------------------------------------
# HTTP helpers
# ---------------------------------------------------------------------------
def post(server: str, path: str, fields: dict) -> dict:
    url = server.rstrip("/") + path
    data = urllib.parse.urlencode(fields).encode()
    req = urllib.request.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            body = resp.read().decode()
            try:
                return json.loads(body)
            except Exception:
                return {"raw": body}
    except urllib.error.HTTPError as e:
        body = e.read().decode()
        print(f"  HTTP {e.code}: {body}")
        return {}
    except urllib.error.URLError as e:
        print(f"  Connection error: {e.reason}")
        print(f"  Is the server running at {server}?")
        return {}


def pretty(data: dict):
    if not data:
        return
    print(json.dumps(data, indent=2))


# ---------------------------------------------------------------------------
# Commands
# ---------------------------------------------------------------------------
def cmd_allheroes(cfg, level=90, star=5):
    print(f"\n>> Grant all heroes (level={level}, star={star}) ...")
    result = post(cfg["server"], "/cheat/allheroes", {
        "SessionId": cfg["session_id"],
        "Level": level,
        "Star": star,
    })
    pretty(result)
    if "HeroesAdded" in result:
        print(f"\n  Added {result['HeroesAdded']} heroes.")


def cmd_uwut(cfg):
    print("\n>> Equip UW + UT1-4 on all heroes ...")
    result = post(cfg["server"], "/cheat/uwut", {
        "SessionId": cfg["session_id"],
    })
    pretty(result)
    if "SlotsEquipped" in result:
        print(
            f"\n  Equipped {result['SlotsEquipped']} slots across {result['HeroesProcessed']} heroes.")


def cmd_unlock(cfg):
    print("\n>> Unlock all chapters (1-11) + skip tutorials + set team level 90 ...")
    result = post(cfg["server"], "/cheat/unlock", {
        "SessionId": cfg["session_id"],
    })
    pretty(result)


def cmd_currency(cfg, gold=0, gem=0, stamina=0):
    if gold == 0 and gem == 0 and stamina == 0:
        print("  Nothing to add (all values are 0).")
        return
    print(
        f"\n>> Add currency (gold={gold:,}, gem={gem:,}, stamina={stamina:,}) ...")
    result = post(cfg["server"], "/cheat/currency", {
        "SessionId": cfg["session_id"],
        "Gold": gold,
        "Gem": gem,
        "Stamina": stamina,
    })
    pretty(result)
    if "NewGold" in result:
        print(
            f"\n  New balances — Gold: {result['NewGold']:,}  Gem: {result['NewGem']:,}  Stamina: {result['NewStamina']:,}")


def cmd_hero(cfg, hero_id, level=1, star=1):
    print(f"\n>> Add hero {hero_id} (level={level}, star={star}) ...")
    result = post(cfg["server"], "/cheat/hero", {
        "SessionId": cfg["session_id"],
        "HeroId": hero_id,
        "Level": level,
        "Star": star,
    })
    pretty(result)


def cmd_level(cfg, level):
    print(f"\n>> Set team level to {level} ...")
    result = post(cfg["server"], "/cheat/level", {
        "SessionId": cfg["session_id"],
        "Level": level,
    })
    pretty(result)


def cmd_reset(cfg, keep_heroes=False):
    print(f"\n>> Reset account (keep_heroes={keep_heroes}) ...")
    result = post(cfg["server"], "/cheat/reset", {
        "SessionId": cfg["session_id"],
        "KeepHeroes": "true" if keep_heroes else "false",
    })
    pretty(result)


# ---------------------------------------------------------------------------
# Interactive menu
# ---------------------------------------------------------------------------
def require_session(cfg) -> bool:
    if not cfg.get("session_id"):
        print("\n  No session ID set. Log into the game first, then run:")
        print("    python gm.py session <YOUR_SESSION_ID>")
        return False
    return True


def prompt_int(label: str, default: int) -> int:
    raw = input(f"  {label} [{default}]: ").strip()
    return int(raw) if raw else default


def interactive(cfg):
    print("\n========================================")
    print("  King's Raid GM CLI")
    print("========================================")
    print(f"  Server  : {cfg['server']}")
    sid = cfg.get("session_id") or "(not set)"
    print(f"  Session : {sid}")
    print()

    MENU = [
        ("1", "Grant all heroes",                 "allheroes"),
        ("2", "Equip all UW/UT on heroes",         "uwut"),
        ("3", "Unlock all chapters + tutorials",  "unlock"),
        ("4", "Add currency",                     "currency"),
        ("5", "Add a single hero",                "hero"),
        ("6", "Set team level",                   "level"),
        ("7", "Reset account",                    "reset"),
        ("8", "Change session ID",                "session"),
        ("9", "Change server URL",                "server"),
        ("q", "Quit",                             "quit"),
    ]

    for key, label, _ in MENU:
        print(f"  [{key}] {label}")

    choice = input("\n  > ").strip().lower()
    action = next((a for k, _, a in MENU if k == choice), None)

    if action is None or action == "quit":
        return

    # Commands that need a session
    if action not in ("session", "server", "quit"):
        if not require_session(cfg):
            return

    if action == "allheroes":
        level = prompt_int("Level", 90)
        star = prompt_int("Star", 5)
        cmd_allheroes(cfg, level, star)

    elif action == "uwut":
        cmd_uwut(cfg)

    elif action == "unlock":
        cmd_unlock(cfg)

    elif action == "currency":
        gold = prompt_int("Gold to add", 999999)
        gem = prompt_int("Gems to add", 9999)
        stamina = prompt_int("Stamina to add", 999)
        cmd_currency(cfg, gold, gem, stamina)

    elif action == "hero":
        raw = input("  Hero ID: ").strip()
        if not raw.isdigit():
            print("  Invalid hero ID.")
            return
        hero_id = int(raw)
        level = prompt_int("Level", 80)
        star = prompt_int("Star", 5)
        cmd_hero(cfg, hero_id, level, star)

    elif action == "level":
        level = prompt_int("Team level", 90)
        cmd_level(cfg, level)

    elif action == "reset":
        confirm = input("  Type YES to confirm account reset: ").strip()
        if confirm != "YES":
            print("  Cancelled.")
            return
        keep = input("  Keep heroes? [y/N]: ").strip().lower() == "y"
        cmd_reset(cfg, keep)

    elif action == "session":
        raw = input("  New session ID: ").strip()
        if raw:
            cfg["session_id"] = raw
            save_config(cfg)
            print(f"  Session saved.")

    elif action == "server":
        raw = input(f"  Server URL [{cfg['server']}]: ").strip()
        if raw:
            cfg["server"] = raw.rstrip("/")
            save_config(cfg)
            print(f"  Server updated.")


# ---------------------------------------------------------------------------
# CLI entry point
# ---------------------------------------------------------------------------
def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        prog="gm.py",
        description="King's Raid GM cheat CLI",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    sub = p.add_subparsers(dest="command")

    sub.add_parser("session", help="Save a session ID").add_argument(
        "id", help="Session ID from game login")

    ah = sub.add_parser("allheroes", help="Grant all 103 core heroes")
    ah.add_argument("--level", type=int, default=90)
    ah.add_argument("--star",  type=int, default=5)

    sub.add_parser("uwut", help="Equip UW + UT1-4 on all heroes")

    sub.add_parser("unlock", help="Unlock chapters 1-11 + skip tutorials")

    cur = sub.add_parser("currency", help="Add currency")
    cur.add_argument("--gold",    type=int, default=999999)
    cur.add_argument("--gem",     type=int, default=9999)
    cur.add_argument("--stamina", type=int, default=999)

    h = sub.add_parser("hero", help="Add a single hero by ID")
    h.add_argument("hero_id", type=int)
    h.add_argument("--level", type=int, default=80)
    h.add_argument("--star",  type=int, default=5)

    lv = sub.add_parser("level", help="Set team level")
    lv.add_argument("level", type=int)

    rs = sub.add_parser("reset", help="Reset account")
    rs.add_argument("--keep-heroes", action="store_true", default=False)

    return p


def main():
    cfg = load_config()
    parser = build_parser()

    # No args → interactive menu
    if len(sys.argv) == 1:
        try:
            interactive(cfg)
        except KeyboardInterrupt:
            print()
        return

    args = parser.parse_args()

    if args.command == "session":
        cfg["session_id"] = args.id
        save_config(cfg)
        print(f"Session ID saved: {args.id}")
        return

    if not cfg.get("session_id"):
        print("No session ID configured. Run: python gm.py session <SESSION_ID>")
        sys.exit(1)

    if args.command == "allheroes":
        cmd_allheroes(cfg, args.level, args.star)
    elif args.command == "uwut":
        cmd_uwut(cfg)
    elif args.command == "unlock":
        cmd_unlock(cfg)
    elif args.command == "currency":
        cmd_currency(cfg, args.gold, args.gem, args.stamina)
    elif args.command == "hero":
        cmd_hero(cfg, args.hero_id, args.level, args.star)
    elif args.command == "level":
        cmd_level(cfg, args.level)
    elif args.command == "reset":
        cmd_reset(cfg, args.keep_heroes)
    else:
        parser.print_help()


if __name__ == "__main__":
    main()
