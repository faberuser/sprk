using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUtil;
using NGame2.NUtil.NConditionChecker;
using NShared;
using NVespa;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class TargetManager : IndicatorManager<DummyNodeIndicatorData>, ITutorialListener
{
	public override NodeIndicator<DummyNodeIndicatorData> CreateIndicator(WorldMapEventInfo worldMapEventInfo)
	{
		return null;
	}

	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		CreateTargetIndicator();
		SingletonUpdater<TutorialManager>.instance.AddListener(this);
	}

	public override void Close()
	{
		SingletonUpdater<TutorialManager>.instance.RemoveListener(this);
		base.Close();
	}

	private bool CheckCanVisit(int chapterIndex, int dungeonIndex, IncreaseDic<int, MapNode> nodeDic)
	{
		foreach (KeyValuePair<int, MapNode> item in (IEnumerable<KeyValuePair<int, MapNode>>)nodeDic)
		{
			if (item.Value.ChapterIndex == chapterIndex && item.Value.DungeonIndex == dungeonIndex)
			{
				return item.Value.CanVisit;
			}
		}
		return false;
	}

	private void CreateTargetIndicator()
	{
		ClearIndicatorImmediately(eatEffect: false);
		if (!SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int num = FindLastVisitableNode(chapterIndex);
			if (num != 0)
			{
				DummyNodeIndicator dummyNodeIndicator = new DummyNodeIndicator(chapterIndex, num);
				dummyNodeIndicator.Open(_eventManager.Mediator, "MoveTarget2", eatEffectOnClose: false);
				_indicatorList.Add(dummyNodeIndicator);
				dummyNodeIndicator.EffectGo.SetActive(value: false);
			}
		}
	}

	private int FindLastVisitableNode(int chapterIndex)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data.Type != ChapterType.WorldMap)
		{
			return 0;
		}
		HashSet<int> hashSet = new HashSet<int>();
		int[] array = new int[1] { data.StartNodeIndex };
		int num = 0;
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			num = FindLastVisitableNode(chapterIndex, array);
			if (num == 0)
			{
				return 0;
			}
			if (hashSet.Contains(num))
			{
				return num;
			}
			hashSet.Add(num);
			CampaignNodeData nodeOpenBy = GetNodeOpenBy(chapterIndex, num);
			if (nodeOpenBy == null)
			{
				return num;
			}
			array = new int[1] { nodeOpenBy.DungeonIndex };
		}
		return num;
	}

	private CampaignNodeData GetNodeOpenBy(int chapterIndex, int reqNodeIndex)
	{
		IList<CampaignNodeData> allChapterData = GameRoot.Instance.CampaignNodeDataContainer.GetAllChapterData(chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allChapterData); i++)
		{
			if (SingletonAuto<ConditionChecker>.instance.CheckCondition<CompleteDungeon>(allChapterData[i]) && (int.TryParse(allChapterData[i].ReqCondition[2], out var result) & bool.TryParse(allChapterData[i].ReqCondition[3], out var result2)) && result2 && result == reqNodeIndex)
			{
				return allChapterData[i];
			}
		}
		return null;
	}

	private int FindLastVisitableNode(int chapterIndex, int[] nextNodeIndices)
	{
		int result = 0;
		HashSet<int> hashSet = new HashSet<int>();
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(nextNodeIndices))
		{
			int[] array = nextNodeIndices;
			nextNodeIndices = null;
			foreach (int num in array)
			{
				if (num == 0 || hashSet.Contains(num))
				{
					continue;
				}
				hashSet.Add(num);
				IncreaseDic<int, MapNode> nodeDictionary = _eventManager.Mediator.NodeGraph.GetNodeDictionary();
				if (CheckCanVisit(chapterIndex, num, nodeDictionary))
				{
					CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, num);
					if (data.Function != NodeFunctionType.None)
					{
						result = num;
					}
					if (!__Extension_Method_ICollection__.IsNullOrEmpty(data.NextNodeIndex))
					{
						nextNodeIndices = data.NextNodeIndex;
					}
					break;
				}
			}
		}
		return result;
	}

	private void CreateTargetIndicator2()
	{
		ClearIndicatorImmediately(eatEffect: false);
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data.Type != ChapterType.WorldMap)
		{
			return;
		}
		int num = data.StartNodeIndex;
		int num2 = 0;
		while (num != 0 && CheckCanVisit(chapterIndex, num, _eventManager.Mediator.NodeGraph.GetNodeDictionary()))
		{
			CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, num);
			if (data2.Function != NodeFunctionType.None)
			{
				num2 = num;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(data2.NextNodeIndex))
			{
				break;
			}
			num = data2.NextNodeIndex[0];
		}
		if (num2 != 0)
		{
			DummyNodeIndicator dummyNodeIndicator = new DummyNodeIndicator(chapterIndex, num2);
			dummyNodeIndicator.Open(_eventManager.Mediator, "MoveTarget2", eatEffectOnClose: false);
			_indicatorList.Add(dummyNodeIndicator);
			dummyNodeIndicator.EffectGo.SetActive(value: false);
		}
	}

	public override void OnNodeChanged(int dungeonIndex)
	{
		base.OnNodeChanged(dungeonIndex);
		CreateTargetIndicator();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		foreach (NodeIndicator<DummyNodeIndicatorData> indicator in _indicatorList)
		{
			if (indicator != null && !(indicator.EffectGo == null) && !indicator.EffectGo.activeInHierarchy)
			{
				MapNode nodeByDungeonIndex = _eventManager.Mediator.NodeGraph.GetNodeByDungeonIndex(indicator.DungeonIndex);
				indicator.EffectGo.SetActive(nodeByDungeonIndex.IsEffectActivated());
			}
		}
	}

	void ITutorialListener.OnTutorialBegin(TutorialData tutorialData)
	{
		ClearIndicatorImmediately(eatEffect: false);
	}

	void ITutorialListener.OnTutorialEnd(int tutorialIndex, Action onEnd)
	{
		if (!SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
		{
			CreateTargetIndicator();
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
