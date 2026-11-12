using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class FirstRewardManager : IEventManagerComponent
{
	private EventManager _eventManager;

	private List<FirstRewardInfo> _rewardList = new List<FirstRewardInfo>();

	private void Open(EventManager eventManager)
	{
		_eventManager = eventManager;
		IncreaseDic<int, MapNode> nodeDictionary = _eventManager.Mediator.NodeGraph.GetNodeDictionary();
		for (int i = 0; i < nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = nodeDictionary.GetValueByIndex(i);
			if (valueByIndex == null)
			{
				continue;
			}
			int chapterIndex = valueByIndex.ChapterIndex;
			int dungeonIndex = valueByIndex.DungeonIndex;
			int linkedDungeonIndex = valueByIndex.LinkedDungeonIndex;
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex) || !valueByIndex.CanVisit)
			{
				continue;
			}
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, linkedDungeonIndex);
			if (data != null && data.FirstRewardIndex != 0)
			{
				RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.FirstRewardIndex);
				if (data2 != null && data2.GemMin > 0)
				{
					FirstRewardInfo firstRewardInfo = FirstRewardInfo.PrefabAuto.Factory.Create(valueByIndex.transform);
					Vespa_Unity_Transform_Extension_Method.ResetLocalTM(firstRewardInfo.transform);
					firstRewardInfo.Open(data2.GemMin);
					_rewardList.Add(firstRewardInfo);
				}
			}
		}
	}

	private void Close()
	{
		FactoryBase.ReleaseAndClearList(_rewardList);
	}

	void IEventManagerComponent.Open(EventManager eventManager)
	{
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
	}

	void IEventManagerComponent.OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd)
	{
	}
}
