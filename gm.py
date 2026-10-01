#!/usr/bin/env python3
"""
sprk GM CLI
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
import sqlite3
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
def cmd_maxheroes(source_db, output_db):
    """Create an isolated, reproducible combat fixture without altering the source."""
    source, output = Path(source_db).resolve(), Path(output_db).resolve()
    if source == output or output.exists():
        raise ValueError("Output must be a new database path, different from the source")
    root = Path(__file__).parent
    shop = json.loads((root / 'tables/HeroShopSupport.json').read_text())
    ext = json.loads((root / 'tables/ExtensionSupport.json').read_text())
    inv = json.loads((root / 'tables/InventorySupport.json').read_text())
    star = max(shop['Stars'], key=lambda r: (r['Transcended'], r['Star']))
    equipment = {r['ItemIndex']: r for r in ext['EquipItem']}
    details = {r['DetailIndex']: r for r in ext['EquipItemDetail']}
    with sqlite3.connect(source.as_uri() + '?mode=ro', uri=True) as src, sqlite3.connect(output) as db:
        src.backup(db)
        heroes = db.execute('SELECT account_id,hero_index FROM heroes').fetchall()
        report = dict(source=str(source), output=str(output), test_only=True, heroes=[])
        for account, hero in heroes:
            creature = shop['Heroes'].get(str(hero))
            if not creature:
                continue
            level = star['MaxHeroLevel']
            skill = min(level, 91)
            db.execute('UPDATE heroes SET level=?,star=?,transcend=?,exp=0,skill_level_1=?,skill_level_2=?,skill_level_3=?,skill_level_4=? WHERE account_id=? AND hero_index=?',
                       (level,star['Star'],star['Transcended'],skill,skill,skill,skill,account,hero))
            old = db.execute('SELECT data FROM hero_details WHERE account_id=? AND hero_index=?',(account,hero)).fetchone()
            extra = json.loads(old[0]) if old else {}
            for slot in range(1,5): extra[f'SkillExtend{slot}'] = int(shop['Constants']['MaxSkillExtend'])
            choices, spent = [], 0
            budget = sum(creature['GetTranscendSkillPoint'] or [])
            for wanted in creature.get('TranscendRecommend1') or []:
                for tier in range(1,6):
                    skills = creature.get(f'TranscendSkill{tier}') or []
                    if wanted not in skills: continue
                    pos = skills.index(wanted); code = tier * 10 + pos
                    cost = creature[f'ReqSkillPoint{tier}'][pos]
                    if code not in choices and spent + cost <= budget and not (tier == 3 and any(c//10==3 and c%10//2==pos//2 for c in choices)):
                        choices.append(code); spent += cost
            extra.update(ApplySkillPage=1, TranscendSkillPage1=json.dumps(choices))
            # Rune pages, rather than legacy equipment rune columns, are used by this client.
            groups = (["RUNE_VITALITY"] * 3 if creature['TagType'] in (3,5)
                      else ["RUNE_FIERCE"] * 3) + ["RUNE_STAMINA", "RUNE_LIFE"]
            rune_page = []
            for rune_slot, group in enumerate(groups, 1):
                candidates = [r for r in ext['RuneItem'] if r['GroupCode'] == group
                              and rune_slot in r['SlotTypes']]
                if not candidates: raise ValueError(f'No compatible rune for slot {rune_slot}')
                rune = max(candidates, key=lambda r: (inv['Items'][str(r['ItemIndex'])]['Grade'],
                                                     'LEGEND_RUNE' in (r.get('Tag') or []), -r['ItemIndex']))
                rune_page.append(dict(HeroIndex=hero,RunePage=1,SlotNum=rune_slot,
                                      ItemIndex=rune['ItemIndex'],EquipItemSlotIndex=0))
            other_pages = [r for r in extra.get('HeroRunePageInfos', []) if r.get('RunePage') != 1]
            extra.update(ApplyRunePage=1, HeroRunePageInfos=rune_page + other_pages)

            db.execute('INSERT OR REPLACE INTO hero_details(account_id,hero_index,data) VALUES(?,?,?)',(account,hero,json.dumps(extra)))
            loadout = {1:1000+hero,7:103000+hero,8:102000+hero,9:101000+hero,10:104000+hero}
            for part in (2,3,4,6):
                candidates = [r for r in equipment.values() if r['PartType']==part and r['Tier']==8
                              and creature['TagType'] in (r.get('TagType') or [])
                              and r['EquipCode'][0].endswith('_8_6_2')]
                if not candidates: raise ValueError(f'No T8 dragon equipment for hero {hero}, part {part}')
                # Ring provides HP for tanks/healers; earrings provide attack for damage dealers.
                preferred = 'RING' if creature['TagType'] in (3,5) else 'EARRING'
                candidates.sort(key=lambda r:(preferred not in r['EquipCode'][0],r['ItemIndex']))
                loadout[part] = candidates[0]['ItemIndex']
            equipped = []
            for part,item in sorted(loadout.items()):
                meta = equipment.get(item)
                if not meta: raise ValueError(f'Missing equipment {item}')
                detail = details[meta['DetailIndex']]
                eq_level = min(level, detail['MaxLevel']) if detail['MaxLevelByHero'] and detail['MaxLevel'] else detail['MaxLevel']
                slot = db.execute('INSERT INTO equip_items(account_id,item_index,star,level,identified,rune_slot_count) VALUES(?,?,?,?,1,?)',
                                  (account,item,detail['MaxStar'],eq_level,meta['MaxRuneCount'])).lastrowid
                available = list(meta.get('OptionIndex') or [])
                for group in meta.get('OptionGroupIndex') or []: available.extend(inv['OptionGroups'].get(str(group),[]))
                available = list(dict.fromkeys(available))
                # Prefer attack, HP, crit and speed when those rolls are legal for this item.
                available.sort(key=lambda v:([1,2,5,8].index(v//100) if v//100 in [1,2,5,8] else 10,v))
                for i,option in enumerate(available[:detail['OptionCount']],1):
                    db.execute(f'UPDATE equip_items SET option_index_{i}=?,option_step_{i}=? WHERE slot_index=?',
                               (option,inv['Options'][str(option)]['Steps'],slot))
                db.execute(f'UPDATE heroes SET equip_item_slot_index_{part}=? WHERE account_id=? AND hero_index=?',(slot,account,hero))
                if part == 1 and meta['IsOpenSoulWeapon']:
                    soul = dict(EquipItemSlotIndex=slot,ItemIndex=item,
                                Grade=max(r['Grade'] for r in ext['SoulWeaponUpgrade']),
                                ReinforceLevel=max(r['ReinforceLevel'] for r in ext['SoulWeaponReinforce']),
                                ReinforceRatio=0,Exp=0,OptionRatio1=500,OptionRatio2=500,
                                OptionBonusRatio1=0,OptionBonusRatio2=0,RenewOptionCount=0,
                                CreatedTime='2026-10-01 00:00:00')
                    db.execute('INSERT OR REPLACE INTO extension_state(account_id,kind,idx,data) VALUES(?,?,?,?)',
                               (account,'soul',slot,json.dumps(soul)))
                equipped.append(dict(part=part,item=item,star=detail['MaxStar'],level=eq_level))
            report['heroes'].append(dict(account=account,hero=hero,level=level,star=star['Star'],transcend=star['Transcended'],skill=skill,perks=choices,runes=rune_page,equipment=equipped))
        db.commit()
    report['scope'] = 'Max core hero progression, skill extensions, recommended perks within native budget, 5-star UW/UT, T8 dragon gear, A2/20 soul weapons. Top-grade compatible attack/HP rune page. Existing artifacts retained.'
    output.with_suffix('.fixture.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(dict(database=str(output),heroes=len(report['heroes']),report=str(output.with_suffix('.fixture.json')))))

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
    print("  sprk GM CLI")
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
        description="sprk GM cheat CLI",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    sub = p.add_subparsers(dest="command")
    mh = sub.add_parser('maxheroes', help='Create a separate max-core-hero raid test database')
    mh.add_argument('--source-db', required=True)
    mh.add_argument('--output-db', required=True)

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

    if args.command == 'maxheroes':
        cmd_maxheroes(args.source_db, args.output_db)
        return

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
