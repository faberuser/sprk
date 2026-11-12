using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class VisitRewardManager : IndicatorManager<DummyNodeIndicatorData>
{
	public override NodeIndicator<DummyNodeIndicatorData> CreateIndicator(WorldMapEventInfo worldMapEventInfo)
	{
		return null;
	}

	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		CreateVisitRewardIndicators();
	}

	public override void Visit()
	{
		base.Visit();
		ReceiveVisitReward();
	}

	private void CreateVisitRewardIndicators()
	{
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
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data != null && data.VisitRewardIndex != 0)
			{
				ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapterIndex, dungeonIndex);
				if ((chapterDungeonInfo == null || string.IsNullOrEmpty(chapterDungeonInfo.VisitedTime)) && valueByIndex.CanVisit)
				{
					DummyNodeIndicator dummyNodeIndicator = new DummyNodeIndicator(valueByIndex.ChapterIndex, valueByIndex.DungeonIndex);
					dummyNodeIndicator.Open(_eventManager.Mediator, "Box", eatEffectOnClose: true);
					_indicatorList.Add(dummyNodeIndicator);
				}
			}
		}
	}

	private void ReceiveVisitReward()
	{
	}

	public override void OnNodeChanged(int dungeonIndex)
	{
		IncreaseDic<int, MapNode> nodeDictionary = _eventManager.Mediator.NodeGraph.GetNodeDictionary();
		for (int i = 0; i < nodeDictionary.Count; i++)
		{
			MapNode node = nodeDictionary.GetValueByIndex(i);
			if (node == null || node.DungeonIndex != dungeonIndex)
			{
				continue;
			}
			int chapterIndex = node.ChapterIndex;
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data != null && data.VisitRewardIndex != 0)
			{
				ChapterDungeonInfo chapterDungeonInfo = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.Get(chapterIndex, dungeonIndex);
				if ((chapterDungeonInfo == null || string.IsNullOrEmpty(chapterDungeonInfo.VisitedTime)) && node.CanVisit && _indicatorList.Find((NodeIndicator<DummyNodeIndicatorData> e) => e.ChapterIndex == node.ChapterIndex && e.DungeonIndex == node.DungeonIndex) == null)
				{
					DummyNodeIndicator dummyNodeIndicator = new DummyNodeIndicator(node.ChapterIndex, node.DungeonIndex);
					dummyNodeIndicator.Open(_eventManager.Mediator, "Box", eatEffectOnClose: true);
					_indicatorList.Add(dummyNodeIndicator);
				}
			}
		}
	}
}
