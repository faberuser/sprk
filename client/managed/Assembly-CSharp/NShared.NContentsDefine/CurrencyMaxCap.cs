using System.Collections.Generic;

namespace NShared.NContentsDefine;

[JsonSerializable]
public class CurrencyMaxCap : ContentsDefine.IDefine
{
	public Dictionary<CurrencyType, long> MaxCap { get; set; }

	public CurrencyMaxCap()
	{
		MaxCap = new Dictionary<CurrencyType, long>();
		MaxCap.Add(CurrencyType.PvpCoin, 0L);
		MaxCap.Add(CurrencyType.GloryPoint, 0L);
		MaxCap.Add(CurrencyType.RankingPoint, 0L);
		MaxCap.Add(CurrencyType.OrdealArenaPoint, 0L);
		MaxCap.Add(CurrencyType.RaidPoint, 0L);
		MaxCap.Add(CurrencyType.WorldBossPoint, 0L);
		MaxCap.Add(CurrencyType.GuildPoint, 0L);
		MaxCap.Add(CurrencyType.GuildArenaPoint, 0L);
	}

	public CurrencyType GetCurrencyTypeOnEndBattle(MatchArenaType arenaType)
	{
		return arenaType switch
		{
			MatchArenaType.Normal => CurrencyType.PvpCoin, 
			MatchArenaType.BanPick => CurrencyType.GloryPoint, 
			MatchArenaType.WorldBanPick => CurrencyType.GloryPoint, 
			MatchArenaType.Ordeal => CurrencyType.OrdealArenaPoint, 
			_ => CurrencyType.None, 
		};
	}

	public CurrencyType GetCurrencyTypeOnEndBattle(RaidType raidType)
	{
		switch (raidType)
		{
		case RaidType.DragonNormal:
		case RaidType.DragonNormalSingle:
		case RaidType.DragonHard:
		case RaidType.DragonHardSingle:
		case RaidType.FieldRaid:
		case RaidType.FieldRaidSingle:
		case RaidType.TechnoMagic:
		case RaidType.TechnoMagicSingle:
		case RaidType.TechnoEnchant:
		case RaidType.TechnoEnchantSingle:
			return CurrencyType.RaidPoint;
		case RaidType.GuildSuppress:
		case RaidType.GuildSuppressSingle:
			return CurrencyType.GuildPoint;
		default:
			return CurrencyType.None;
		}
	}
}
