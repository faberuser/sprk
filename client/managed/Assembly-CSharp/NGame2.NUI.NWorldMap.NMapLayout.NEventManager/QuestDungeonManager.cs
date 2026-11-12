using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class QuestDungeonManager : IEventManagerComponent
{
	private EventManager _eventManager;

	private List<QuestDungeonInfo> _questDungeonList = new List<QuestDungeonInfo>();

	private int _hideDungeonIndex;

	private void Open(EventManager eventManager)
	{
		_eventManager = eventManager;
		_hideDungeonIndex = 0;
	}

	private void Close()
	{
		FactoryBase.ReleaseAndClearList(_questDungeonList);
	}

	public QuestDungeonInfo GetQuestDungeonInfo(int chapterIndex, int dungeonIndex)
	{
		for (int i = 0; i < _questDungeonList.Count; i++)
		{
			QuestDungeonInfo questDungeonInfo = _questDungeonList[i];
			if (questDungeonInfo.QuestDungeonData.ChapterIndex == chapterIndex && questDungeonInfo.QuestDungeonData.DungeonIndex == dungeonIndex)
			{
				return questDungeonInfo;
			}
		}
		return null;
	}

	void IEventManagerComponent.Open(EventManager eventManager)
	{
		Open(eventManager);
	}

	void IEventManagerComponent.Setup(WorldMapEventInfo worldMapEventInfo)
	{
		if (worldMapEventInfo == null || worldMapEventInfo.EventType != NShared.NWorldMap.EventType.QuestDungeon)
		{
			return;
		}
		Mediator mediator = _eventManager.Mediator;
		int chapterIndex = worldMapEventInfo.ChapterIndex;
		if (chapterIndex != NGame2.NAccount.UserCampaignManager.ChapterIndex)
		{
			return;
		}
		int dungeonIndex = worldMapEventInfo.DungeonIndex;
		QuestDungeon eventData = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<QuestDungeon>(chapterIndex, dungeonIndex);
		if (eventData != null)
		{
			MapNode nodeByDungeonIndex = mediator.NodeGraph.GetNodeByDungeonIndex(dungeonIndex);
			if (!(nodeByDungeonIndex == null))
			{
				QuestDungeonInfo questDungeonInfo = QuestDungeonInfo.PrefabAuto.Factory.Create(mediator.Layout.EventRoot);
				questDungeonInfo.Open(eventData);
				questDungeonInfo.transform.position = nodeByDungeonIndex.transform.position;
				questDungeonInfo.transform.localScale = Vector3.one;
				_questDungeonList.Add(questDungeonInfo);
				_eventManager.SortPanel(questDungeonInfo);
			}
		}
	}

	void IEventManagerComponent.Remove(WorldMapEventInfo worldMapEventInfo, bool eatEffect)
	{
		if (worldMapEventInfo != null)
		{
			int chapterIndex = worldMapEventInfo.ChapterIndex;
			int dungeonIndex = worldMapEventInfo.DungeonIndex;
			QuestDungeonInfo questDungeonInfo = GetQuestDungeonInfo(chapterIndex, dungeonIndex);
			if (questDungeonInfo != null)
			{
				_questDungeonList.Remove(questDungeonInfo);
				FactoryBase.ReleaseComponent(questDungeonInfo);
			}
		}
	}

	void IEventManagerComponent.ChainUpdate()
	{
		List<QuestDungeonInfo> list = null;
		for (int i = 0; i < _questDungeonList.Count; i++)
		{
			QuestDungeonInfo questDungeonInfo = _questDungeonList[i];
			if (questDungeonInfo.GetRemainTime() <= 0)
			{
				if (list == null)
				{
					list = new List<QuestDungeonInfo>();
				}
				list.Add(questDungeonInfo);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
		{
			QuestDungeonInfo questDungeonInfo2 = list[j];
			_questDungeonList.Remove(questDungeonInfo2);
			FactoryBase.ReleaseComponent(questDungeonInfo2);
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (dungeonIndex != _hideDungeonIndex)
		{
			QuestDungeonInfo questDungeonInfo3 = GetQuestDungeonInfo(chapterIndex, _hideDungeonIndex);
			if (questDungeonInfo3 != null)
			{
				questDungeonInfo3.Show(show: true);
			}
			QuestDungeonInfo questDungeonInfo4 = GetQuestDungeonInfo(chapterIndex, dungeonIndex);
			if (questDungeonInfo4 != null)
			{
				questDungeonInfo4.Show(show: false);
			}
			_hideDungeonIndex = dungeonIndex;
		}
	}

	void IEventManagerComponent.Visit()
	{
	}

	void IEventManagerComponent.Close()
	{
		Close();
	}

	void IEventManagerComponent.OnNodeChanged(int dungeonIndex)
	{
	}

	void IEventManagerComponent.OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd)
	{
	}
}
