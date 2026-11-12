using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class HeroBonusDataContainer : JitDataContainerDefault<HeroBonusDataKey, HeroBonusData>
{
	public const string TABLENAME = "HeroBonusTable";

	public readonly Dictionary<EventBonusType, HashSet<int>> BonusOwnedHeroIndices = new Dictionary<EventBonusType, HashSet<int>>();

	private List<int> BuffedNpcIndices = new List<int>();

	private int lastIndex;

	public override TablePathDefine PathDefine => new TablePathDefine("HeroBonusTable");

	protected override void OnEndLoad_Final()
	{
		BuffedNpcIndices = (from v in GetAllData()
			select v.HeroIndex).Distinct().ToList();
		foreach (HeroBonusData allDatum in GetAllData())
		{
			allDatum.AddBonusTypesHeroIndicse(BonusOwnedHeroIndices);
		}
	}

	public List<int> GetBuffedNpcIndices()
	{
		return BuffedNpcIndices;
	}

	public HeroBonusData GetData(int heroIndex, int heroStar, int heroTranscended)
	{
		return GetData(new HeroBonusDataKey(heroIndex, heroStar, heroTranscended));
	}

	public List<HeroBonusData> GetHeroBonusWithBonusType1(EventBonusType bonusType)
	{
		List<HeroBonusData> list = new List<HeroBonusData>();
		foreach (HeroBonusData allDatum in GetAllData())
		{
			if (allDatum.BonusType1 == bonusType)
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
