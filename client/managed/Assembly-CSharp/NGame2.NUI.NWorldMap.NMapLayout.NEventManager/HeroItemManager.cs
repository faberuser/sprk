using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class HeroItemManager : IEventManagerComponent
{
	private class RewardHeroCache : SingletonAuto<RewardHeroCache>
	{
		private Dictionary<CampaignDungeonKey, List<CreatureData>> _dic = new Dictionary<CampaignDungeonKey, List<CreatureData>>();

		public List<CreatureData> GetRewardHeroList(int chapterIndex, int dungeonIndex)
		{
			CampaignDungeonKey key = new CampaignDungeonKey(chapterIndex, dungeonIndex);
			if (_dic.TryGetValue(key, out var value))
			{
				return value;
			}
			value = CreateRewardHeroList(chapterIndex, dungeonIndex);
			_dic.Add(key, value);
			return value;
		}

		private List<CreatureData> CreateRewardHeroList(int chapterIndex, int dungeonIndex)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data == null)
			{
				return null;
			}
			RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.GetRewardIndexOfDifficulty(DungeonDifficultyType.Easy));
			if (data2 == null)
			{
				return null;
			}
			List<CreatureData> list = new List<CreatureData>();
			List<RewardElementData> rewardItemList = data2.GetRewardItemList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemList); i++)
			{
				RewardElementData rewardElementData = rewardItemList[i];
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
				if (dataByCode != null && dataByCode.Type == ItemType.Hero)
				{
					int valueInt = dataByCode.GetValueInt(0);
					CreatureData data3 = GameRoot.Instance.CreatureDataContainer.GetData(valueInt);
					if (data3 != null)
					{
						list.Add(data3);
					}
				}
			}
			return list;
		}
	}

	private EventManager _eventManager;

	private List<HeroItemInfo> _heroItemList = new List<HeroItemInfo>();

	private void Open(EventManager eventManager)
	{
		if (eventManager == null)
		{
			return;
		}
		_eventManager = eventManager;
		IncreaseDic<int, MapNode> nodeDictionary = _eventManager.Mediator.NodeGraph.GetNodeDictionary();
		for (int i = 0; i < nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = nodeDictionary.GetValueByIndex(i);
			if (!(valueByIndex == null))
			{
				int chapterIndex = valueByIndex.ChapterIndex;
				int linkedDungeonIndex = valueByIndex.LinkedDungeonIndex;
				List<CreatureData> rewardHeroList = SingletonAuto<RewardHeroCache>.instance.GetRewardHeroList(chapterIndex, linkedDungeonIndex);
				if (__Extension_Method_ICollection__.COUNT(rewardHeroList) != 0)
				{
					HeroItemInfo heroItemInfo = HeroItemInfo.PrefabAuto.Factory.Create(valueByIndex.transform);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroItemInfo.transform);
					heroItemInfo.Open(rewardHeroList);
					_heroItemList.Add(heroItemInfo);
				}
			}
		}
	}

	private void Close()
	{
		FactoryBase.ReleaseAndClearList(_heroItemList);
	}

	private void Reload()
	{
		Close();
		Open(_eventManager);
	}

	void IEventManagerComponent.Open(EventManager eventManager)
	{
		Close();
		Open(eventManager);
	}

	void IEventManagerComponent.Setup(WorldMapEventInfo worldMapEventInfo)
	{
	}

	void IEventManagerComponent.Remove(WorldMapEventInfo worldMapEventInfo, bool eatEffect)
	{
	}

	void IEventManagerComponent.Close()
	{
		Close();
	}

	void IEventManagerComponent.Visit()
	{
	}

	void IEventManagerComponent.ChainUpdate()
	{
	}

	void IEventManagerComponent.OnNodeChanged(int dungeonIndex)
	{
		Reload();
	}

	void IEventManagerComponent.OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd)
	{
	}
}
