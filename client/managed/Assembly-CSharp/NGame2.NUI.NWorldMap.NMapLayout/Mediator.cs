using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGameAsset.NWorldMap;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class Mediator : AvatarController.IListener, IAvatarContainer
{
	public int MapWidth = 4096;

	public int MapHeight = 4096;

	private DummyNodeIndicator _moveTargetIndicator;

	public MapComponent MapComponent { get; set; }

	public Avatar Avatar { get; set; }

	public AvatarController AvatarController { get; set; }

	public MapLayout Layout => MapComponent.Layout;

	public NodeGraph NodeGraph => MapComponent.NodeGraph;

	public MapViewer Viewer => MapComponent.Viewer;

	public void OnUnloadMap()
	{
		ClearMoveTargetIndicator();
	}

	public void OnLoadMap()
	{
		ValidateChapterIndex();
	}

	public void Close()
	{
		ClearMoveTargetIndicator();
	}

	public Vector3 ConvertNodePosition(int nodePositionX, int nodePositionY)
	{
		return new Vector3(nodePositionX - MapWidth / 2, -nodePositionY + MapHeight / 2);
	}

	public void ValidateChapterIndex()
	{
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex) == null)
		{
			VespaDebug.LogError("Chapter data not found : " + NGame2.NAccount.UserCampaignManager.ChapterIndex);
			IList<CampaignChapterData> allData = GameRoot.Instance.CampaignChapterDataContainer.GetAllData();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(allData))
			{
				VespaDebug.LogError("Move to chapter : " + allData[0].Index);
				NGame2.NAccount.UserCampaignManager.ChapterIndex = allData[0].Index;
			}
		}
	}

	private void ClearMoveTargetIndicator()
	{
		if (_moveTargetIndicator != null)
		{
			_moveTargetIndicator.CloseImmediately();
			_moveTargetIndicator = null;
		}
	}

	int AvatarController.IListener.GetChapterIndex()
	{
		return NGame2.NAccount.UserCampaignManager.ChapterIndex;
	}

	int AvatarController.IListener.GetDungeonIndex()
	{
		return NGame2.NAccount.UserCampaignManager.DungeonIndex;
	}

	void AvatarController.IListener.SetDungeonIndex(int dungeonIndex)
	{
		NGame2.NAccount.UserCampaignManager.DungeonIndex = dungeonIndex;
	}

	Avatar AvatarController.IListener.GetAvatar()
	{
		return Avatar;
	}

	NodeGraph AvatarController.IListener.GetNodeGraph()
	{
		return NodeGraph;
	}

	ScaleData AvatarController.IListener.GetScaleData()
	{
		return MapComponent.ScaleData;
	}

	void AvatarController.IListener.OnMoveBegin(int chapterIndex, int[] dungeonIndices)
	{
		Viewer.ChasePlayer();
		ClearMoveTargetIndicator();
		if (__Extension_Method_ICollection__.COUNT(dungeonIndices) > 0)
		{
			int dungeonIndex = dungeonIndices[__Extension_Method_ICollection__.COUNT(dungeonIndices) - 1];
			if ((bool)NodeGraph.GetNodeByDungeonIndex(dungeonIndex))
			{
				_moveTargetIndicator = new DummyNodeIndicator(chapterIndex, dungeonIndex);
				_moveTargetIndicator.Open(this, "MoveTarget", eatEffectOnClose: true);
			}
		}
		int num = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MapNode nodeByDungeonIndex = NodeGraph.GetNodeByDungeonIndex(num);
		if (nodeByDungeonIndex != null && nodeByDungeonIndex.LinkedDungeonIndex != num)
		{
			num = nodeByDungeonIndex.LinkedDungeonIndex;
		}
		PlayerMoveBegin triggerEvent = new PlayerMoveBegin
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = num
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	void AvatarController.IListener.OnMoveEnd()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int num = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MapNode nodeByDungeonIndex = NodeGraph.GetNodeByDungeonIndex(num);
		if (nodeByDungeonIndex != null && nodeByDungeonIndex.LinkedDungeonIndex != num)
		{
			num = nodeByDungeonIndex.LinkedDungeonIndex;
		}
		ClearMoveTargetIndicator();
		PlayerMoveEnd triggerEvent = new PlayerMoveEnd
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = num
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		SceneAutoBehaviour<LobbyManagement>.instance.OnPlayerMoveEnd();
		SceneAutoBehaviour<WorldMapManagement>.instance.ProcessChapterFlow(chapterIndex, num, ChapterFlowManager.ConditionKeyType.OpenNode);
	}

	void AvatarController.IListener.OnVisitNode(int chapterIndex, int dungeonIndex)
	{
		MapNode nodeByDungeonIndex = NodeGraph.GetNodeByDungeonIndex(dungeonIndex);
		if (nodeByDungeonIndex != null && nodeByDungeonIndex.LinkedDungeonIndex != dungeonIndex)
		{
			dungeonIndex = nodeByDungeonIndex.LinkedDungeonIndex;
		}
		PlayerVisitNode triggerEvent = new PlayerVisitNode
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		Layout.EventManager.ProcessVisitNode();
	}

	void IAvatarContainer.ForEach(Action<Avatar> action)
	{
		if (Avatar != null)
		{
			Vespa_System_Action_Extension_Method.fire(action, Avatar);
		}
	}

	public void OnLongTouch(MapNode node)
	{
		if (!(node == null) && (NGame2.NAccount.UserCampaignManager.ChapterIndex != node.ChapterIndex || NGame2.NAccount.UserCampaignManager.DungeonIndex != node.DungeonIndex) && node.CanVisit && node.NodeData != null && node.NodeData.Function == NodeFunctionType.Battle)
		{
			int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(node.ChapterIndex, node.DungeonIndex);
			SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(node.ChapterIndex, node.DungeonIndex, linkedDungeonIndex, notCurrentNode: true);
		}
	}
}
