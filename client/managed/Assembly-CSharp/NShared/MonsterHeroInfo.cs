using System;
using NVespa;

namespace NShared;

public class MonsterHeroInfo
{
	public HeroInfo HeroInfo { get; set; }

	public int Tier { get; set; }

	public MonsterGradeType Grade { get; set; }

	public bool IsBoss { get; set; }

	public string VisualInfo { get; set; }

	public int WaveSlotIndex { get; set; }

	public string BookCustomIdle { get; set; }

	public static implicit operator HeroInfo(MonsterHeroInfo monsterInfo)
	{
		return monsterInfo?.HeroInfo;
	}

	public long GetMaxHp()
	{
		return GetStat(StatType.MaxHp, (MonsterTierData tierData) => tierData.MaxHp, 9999999999999L);
	}

	private long GetStat(StatType statType, Func<MonsterTierData, long> tierRatioGetter, long fallback)
	{
		if (HeroInfo == null)
		{
			return fallback;
		}
		StatData statData = GameRoot.Instance.CreatureStatDataContainer.GetStatData(HeroInfo.HeroIndex, HeroInfo.Star, HeroInfo.Level, HeroInfo.Transcended, new StatType[1] { statType });
		MonsterTierData data = GameRoot.Instance.MonsterTierDataContainer.GetData(Tier);
		if (data == null)
		{
			VespaDebug.LogError("TierData not found : " + Tier + " heroIndex=" + HeroInfo.HeroIndex);
			return fallback;
		}
		return Convert.ToInt64((decimal)statData.GetStat(statType) * (decimal)tierRatioGetter(data) / 1000m);
	}
}
