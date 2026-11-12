using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class CachedAccountInfo : IAccountInfo
{
	public CachedUserInfo UserInfo { get; set; }

	public Dictionary<int, CachedHeroInfo> HeroInfos { get; set; }

	public Dictionary<int, CachedHeroInfo> AiHeroInfos { get; set; }

	public Dictionary<int, CachedHeroInfo> GroupHeroInfos { get; set; }

	public Dictionary<BattleType, DeckInfo> DeckInfos { get; set; }

	IEnumerable<CachedHeroInfo> IAccountInfo.GetHeroInfos()
	{
		if (HeroInfos != null)
		{
			return HeroInfos.Values;
		}
		return null;
	}

	public int[] GetHeroIndices()
	{
		if (HeroInfos == null || HeroInfos.Count == 0)
		{
			return null;
		}
		int[] array = new int[HeroInfos.Count];
		int num = 0;
		foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in HeroInfos)
		{
			array[num] = heroInfo.Value.HeroIndex;
			num++;
		}
		return array;
	}
}
