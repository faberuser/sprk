using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class DungeonNpcManager : IEventManagerComponent, NpcAvatarManager.IAvatarOwner, IUserObserver<UserManager.UserStatInfo>, IUserObserver
{
	private enum EShakmehAction
	{
		None,
		Appear,
		Idle,
		Disappear
	}

	private EventManager _eventManager;

	private NpcAvatarManager _npcAvatarManager;

	private IncreaseDic<int, MapNode> _nodeDictionary;

	private void Open(EventManager eventManager)
	{
		_eventManager = eventManager;
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		_npcAvatarManager = instance.EventManager.NpcAvatarManager;
		_ = _eventManager.Mediator.MapComponent;
		_nodeDictionary = _eventManager.Mediator.NodeGraph.GetNodeDictionary();
		UpdateNodeNpcAvatar();
		UpdateShakmehNodeNpcAvatar();
	}

	private void UpdateNodeNpcAvatar()
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(i);
			if (!(valueByIndex == null))
			{
				CampaignNodeData nodeData = valueByIndex.NodeData;
				if (nodeData != null)
				{
					_eventManager.NpcAvatarManager.CreateNpcAvatar(this, valueByIndex, nodeData.EntryNPCCode);
				}
			}
		}
	}

	public void UpdateShakmehNodeNpcAvatar()
	{
		for (int i = 0; i < _nodeDictionary.Count; i++)
		{
			MapNode valueByIndex = _nodeDictionary.GetValueByIndex(i);
			if (!(valueByIndex == null))
			{
				CampaignNodeData nodeData = valueByIndex.NodeData;
				if (nodeData != null && !String_Extension_Method.IsNullOrEmpty(nodeData.EntryNPCCode))
				{
					ShakmehNpcAvatarChecker(valueByIndex, nodeData);
				}
			}
		}
	}

	private void ShakmehNpcAvatarChecker(MapNode node, CampaignNodeData nodeData)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(node.ChapterIndex, node.DungeonIndex);
		if (data == null || data.Function != NodeFunctionType.ShakmehDungeon)
		{
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance == null)
		{
			return;
		}
		NpcAvatar npcAvatar = GetNpcAvatar(node.ChapterIndex, node.DungeonIndex);
		if (npcAvatar == null)
		{
			return;
		}
		if (!instance.IsOpenShakmehDungeon)
		{
			npcAvatar.Show(show: false);
			return;
		}
		ShakmehBossData data2 = GameRoot.Instance.ShakmehBossDataContainer.GetData(node.ChapterIndex, node.DungeonIndex);
		if (data2 == null)
		{
			return;
		}
		CampaignNodeNPCData data3 = GameRoot.Instance.CampaignNodeNPCDataContainer.GetData(node.ChapterIndex, node.DungeonIndex);
		if (data3 == null)
		{
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState currShakmehNpcActionState = instance.CurrShakmehNpcActionState;
		ShakmehBossType bossType = data2.BossType;
		float shakmehNodeEffectDelay = instance.ShakmehNodeEffectDelay;
		switch (currShakmehNpcActionState)
		{
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Idle_Middle:
			npcAvatar.Show(bossType == ShakmehBossType.Middle);
			break;
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Idle_Final:
			npcAvatar.Show(bossType == ShakmehBossType.Final);
			break;
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Middle:
			if (bossType == ShakmehBossType.Middle)
			{
				VespaCoroutine.Start(coShakmehAvatarAction(npcAvatar, data3, EShakmehAction.Appear, shakmehNodeEffectDelay), null, null);
			}
			break;
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Final:
			if (bossType == ShakmehBossType.Middle)
			{
				VespaCoroutine.Start(coShakmehAvatarAction(npcAvatar, data3, EShakmehAction.Disappear, 0f), null, null);
			}
			break;
		}
	}

	private NpcAvatar GetNpcAvatar(int chapterIndex, int dungeonIndex)
	{
		return SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>().EventManager.NpcAvatarManager.GetNPCAvatar(chapterIndex, dungeonIndex);
	}

	private void ChangeDropBonusEvent(int chapaterIndex, int dungeonIndex, bool isAdd)
	{
		if (isAdd)
		{
			CreateTarget(dungeonIndex);
		}
		else
		{
			RemoveTarget(chapaterIndex, dungeonIndex);
		}
	}

	private void RemoveTarget(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			_eventManager.NpcAvatarManager.RemoveAvatar(this, data.ChapterIndex, data.DungeonIndex, data.EntryNPCCode);
		}
	}

	private void CreateTarget(int dungeonIndex)
	{
		MapNode nodeByDungeonIndex = _eventManager.Mediator.NodeGraph.GetNodeByDungeonIndex(dungeonIndex);
		if (!(nodeByDungeonIndex == null))
		{
			CampaignNodeData nodeData = nodeByDungeonIndex.NodeData;
			if (nodeData != null)
			{
				_eventManager.NpcAvatarManager.CreateNpcAvatar(this, nodeByDungeonIndex, nodeData.EntryNPCCode);
			}
		}
	}

	private void SetAvatarQuestEffect(NpcAvatar avatar)
	{
	}

	private IEnumerator coShakmehAvatarAction(NpcAvatar avatar, CampaignNodeNPCData campaignNodeNPCData, EShakmehAction eShakmehAction, float delay)
	{
		switch (eShakmehAction)
		{
		case EShakmehAction.Appear:
			avatar.Show(show: false);
			break;
		case EShakmehAction.Disappear:
			avatar.Show(show: true);
			break;
		}
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		switch (eShakmehAction)
		{
		case EShakmehAction.Appear:
			avatar.Show(show: true);
			avatar.SetState(CampaignNodeNPCData.NPCAvatarState.Appear, campaignNodeNPCData.AppearMotionValues, unUseBlenMotion: true);
			break;
		case EShakmehAction.Disappear:
			avatar.Show(show: true);
			avatar.SetState(CampaignNodeNPCData.NPCAvatarState.Dead, campaignNodeNPCData.DeadMotionValues);
			break;
		}
		yield return null;
		if (eShakmehAction == EShakmehAction.Disappear)
		{
			float timeLength = avatar.RenderFacade.AnimationData.Runtime.TimeLength;
			yield return new WaitForSeconds(timeLength);
			avatar.Show(show: false);
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance != null)
		{
			instance.ShakmehDungeonEnterData.SetLocalSaveShakmehPoint();
		}
	}

	void IEventManagerComponent.Open(EventManager eventManager)
	{
		Open(eventManager);
	}

	void IEventManagerComponent.OnChangeDropBonusEvent(int chapterIndex, int dungeonIndex, bool isAdd)
	{
		ChangeDropBonusEvent(chapterIndex, dungeonIndex, isAdd);
	}

	void IEventManagerComponent.Setup(WorldMapEventInfo worldMapEventInfo)
	{
	}

	void IEventManagerComponent.Remove(WorldMapEventInfo worldMapEventInfo, bool eatEffect)
	{
	}

	void IEventManagerComponent.Close()
	{
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

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		throw new NotImplementedException();
	}

	bool IUserObserver.IsGlobal()
	{
		throw new NotImplementedException();
	}
}
