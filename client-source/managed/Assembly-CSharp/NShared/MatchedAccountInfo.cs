namespace NShared;

[JsonSerializable(Javascript = true)]
public class MatchedAccountInfo : CachedAccountInfo
{
	public string Host { get; set; }

	public string ServerGroup { get; set; }

	public string MatchServerHost { get; set; }

	public bool IsNpc { get; set; }

	public MatchArenaType ArenaType { get; set; }

	public TeamType TeamType { get; set; }

	public int Mmr { get; set; }

	public int FightingPower { get; set; }

	public int Rank { get; set; }

	public int LeaderHeroIndex { get; set; }

	public GuildServerInfo[] GuildInfo { get; set; }

	public GuildSkillDataBase[] GuildSkills { get; set; }

	public AccountBuffDataBase[] AccountBuffs { get; set; }

	public ClassBuffDataBase[] ClassBuffDataBases { get; set; }

	public GuildArenaBuffDataBase[] GuildArenaBuffDataBases { get; set; }

	public DefaultExtraStatData[] ExtraStatDataBases { get; set; }

	public PetStatDataBase[] PetStatDataBases { get; set; }

	public PlayerBattleInfo BattleInfo { get; set; }

	public int AvgFightingPower { get; set; }

	public int LuckyDeckIndex { get; set; }

	public long AccountId
	{
		get
		{
			if (base.UserInfo == null)
			{
				return 0L;
			}
			return base.UserInfo.AccountId;
		}
	}

	public string Nick
	{
		get
		{
			if (base.UserInfo == null)
			{
				return string.Empty;
			}
			return base.UserInfo.Nick;
		}
	}

	public int AvatarHeroIndex
	{
		get
		{
			if (base.UserInfo == null)
			{
				return 1;
			}
			return base.UserInfo.AvatarHeroIndex;
		}
	}

	public int TierIndex
	{
		get
		{
			if (base.UserInfo == null)
			{
				return 0;
			}
			return base.UserInfo.TierIndex;
		}
	}
}
