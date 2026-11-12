using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroBonusData : IDataKeySelector<HeroBonusDataKey>
{
	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int HeroStar { get; set; }

	[Key(2)]
	public int HeroTranscended { get; set; }

	[Key(3)]
	public EventBonusType BonusType1 { get; set; }

	[Key(4)]
	public string[] BonusValue1 { get; set; }

	[Key(5)]
	public EventBonusType BonusType2 { get; set; }

	[Key(6)]
	public string[] BonusValue2 { get; set; }

	[Key(7)]
	public EventBonusType BonusType3 { get; set; }

	[Key(8)]
	public string[] BonusValue3 { get; set; }

	[Key(9)]
	public EventBonusType BonusType4 { get; set; }

	[Key(10)]
	public string[] BonusValue4 { get; set; }

	[Key(11)]
	public EventBonusType BonusType5 { get; set; }

	[Key(12)]
	public string[] BonusValue5 { get; set; }

	[Key(13)]
	public string BonusNameKey { get; set; }

	[Key(14)]
	public string BonusDescKey { get; set; }

	[Key(15)]
	public string Icon { get; set; }

	[Key(16)]
	public bool ShowBoosterIconTooltip { get; set; }

	HeroBonusDataKey IDataKeySelector<HeroBonusDataKey>.Key => new HeroBonusDataKey(HeroIndex, HeroStar, HeroTranscended);

	public HeroEventBonusData[] GetHeroBonusTypeDatas(EventBonusType eventBonusType)
	{
		List<HeroEventBonusData> list = new List<HeroEventBonusData>();
		if (BonusType1 == eventBonusType)
		{
			list.Add(new HeroEventBonusData
			{
				BonusType = BonusType1,
				BonusValue = BonusValue1
			});
		}
		if (BonusType2 == eventBonusType)
		{
			list.Add(new HeroEventBonusData
			{
				BonusType = BonusType2,
				BonusValue = BonusValue2
			});
		}
		if (BonusType3 == eventBonusType)
		{
			list.Add(new HeroEventBonusData
			{
				BonusType = BonusType3,
				BonusValue = BonusValue3
			});
		}
		if (BonusType4 == eventBonusType)
		{
			list.Add(new HeroEventBonusData
			{
				BonusType = BonusType4,
				BonusValue = BonusValue4
			});
		}
		if (BonusType5 == eventBonusType)
		{
			list.Add(new HeroEventBonusData
			{
				BonusType = BonusType5,
				BonusValue = BonusValue5
			});
		}
		return list.ToArray();
	}

	public void AddBonusTypesHeroIndicse(Dictionary<EventBonusType, HashSet<int>> container)
	{
		AddBonusTypesHeroIndices(BonusType1, container);
		AddBonusTypesHeroIndices(BonusType2, container);
		AddBonusTypesHeroIndices(BonusType3, container);
		AddBonusTypesHeroIndices(BonusType4, container);
		AddBonusTypesHeroIndices(BonusType5, container);
	}

	private void AddBonusTypesHeroIndices(EventBonusType bonusType, Dictionary<EventBonusType, HashSet<int>> bonusTypeHeroIndices)
	{
		if (bonusType != EventBonusType.None)
		{
			if (!bonusTypeHeroIndices.ContainsKey(bonusType))
			{
				bonusTypeHeroIndices[bonusType] = new HashSet<int>();
			}
			if (!bonusTypeHeroIndices[bonusType].Contains(HeroIndex))
			{
				bonusTypeHeroIndices[bonusType].Add(HeroIndex);
			}
		}
	}
}
