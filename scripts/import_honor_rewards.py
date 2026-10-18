"""Recover fixed Honor payouts from the pre-Doomsday extraction and metadata."""
import argparse
import hashlib
import json
import struct
from pathlib import Path


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def build(archive, tables):
    metadata = archive / "com.vespainteractive.KingsRaid/files/il2cpp/Metadata/global-metadata.dat"
    binary = metadata.read_bytes()
    header = struct.unpack_from("<60I", binary)
    assert header[:2] == (0xFAB11BAF, 24)
    strings, length = header[6:8]

    def string(index):
        return binary[strings + index:binary.index(b"\0", strings + index)].decode()

    def fields(name):
        index = binary.index(name.encode() + b"\0", strings, strings + length) - strings
        entries = [offset for offset in range(header[40], header[40] + header[41], 100)
                   if struct.unpack_from("<I", binary, offset)[0] == index
                   and string(struct.unpack_from("<I", binary, offset + 4)[0]) == "NShared"]
        assert len(entries) == 1, name
        words = struct.unpack_from("<25I", binary, entries[0])
        return [string(struct.unpack_from("<I", binary, header[24] + i * 12)[0])
                .removeprefix("<").removesuffix(">k__BackingField")
                for i in range(words[11], words[11] + (words[20] & 65535))]

    keys = fields("MatchRewardData") + fields("MatchBanPickTierRewardData")
    assert keys == ["TierIndex", "MinRank", "MaxRank", "DailyReward", "SeasonReward",
                    "FirstReward", "ShowMinRank", "ShowMaxRank", "WinReward",
                    "AttendanceReward", "WeeklyReward", "WeeklyBoxReward", "ServerBuff", "ShowRewardIndex"]
    raw = archive / "kingsraid-table-data/output/MessagePackRaw"
    tier_path = raw / "GlobalBanPickTierRewardTable.json"
    reward_path = raw / "RewardTable.json"
    tiers = [dict(zip(keys, row, strict=True)) for row in read(tier_path)]
    indices = {r[key] for r in tiers for key in ("WeeklyBoxReward", "ShowRewardIndex") if r[key] > 0}
    mapping = read(tables / "ItemCodeToIndex.json")
    inventory = read(tables / "InventorySupport.json")
    bundles = []
    for row in read(reward_path):
        if row[0] not in indices:
            continue
        assert not any(row[1:15]), (row[0], "Unexpected fixed currency reward")
        items = []
        for slot in range(20):
            rate, code, minimum, maximum, star_min, star_max, custom = row[15 + slot * 7:22 + slot * 7]
            if rate == 0:
                continue
            assert rate == 1000 and minimum == maximum and not any((star_min, star_max, custom)), row[0]
            code = code[0] if isinstance(code, list) else code
            index = mapping[code]
            assert str(index) in inventory["Items"], (row[0], code)
            items.append({"ItemIndex": index, "ItemCount": minimum, "ItemCode": code})
        assert items, row[0]
        bundles.append({"Index": row[0], "Items": items})
    assert {b["Index"] for b in bundles} == indices
    boosters = [{"Rank": rank, "ItemIndex": mapping[f"BOOSTER_GOLD_BANPICK_REWARD_{suffix}"]}
                for rank, suffix in enumerate(("1ST", "2ND", "3RD"), 1)]
    return {
        "Source": {"Description": "Pre-Doomsday build 1558. Field order verified against archived IL2CPP metadata; fixed RewardData item slots only.",
                   "Sha256": {"global-metadata.dat": hashlib.sha256(binary).hexdigest(),
                              tier_path.name: hashlib.sha256(tier_path.read_bytes()).hexdigest(),
                              reward_path.name: hashlib.sha256(reward_path.read_bytes()).hexdigest()}},
        "Tables": {"HonorTierReward": tiers, "HonorRewardBundle": bundles, "HonorSeasonBooster": boosters},
    }


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--tables", type=Path, default=Path("tables"))
    args = parser.parse_args()
    result = build(args.archive, args.tables)
    (args.tables / "HonorRewards.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print({key: len(value) for key, value in result["Tables"].items()})
