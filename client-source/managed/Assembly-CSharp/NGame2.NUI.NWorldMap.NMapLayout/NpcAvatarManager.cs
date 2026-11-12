using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUtil;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class NpcAvatarManager : IAvatarContainer
{
	public interface IAvatarOwner
	{
	}

	private class AvatarWrapper
	{
		public NpcAvatar Avatar;

		public IAvatarOwner Owner;

		public bool CanControl;

		public CampaignNodeNPCData NodeNPCData;

		public void SetControlNPC(MapNode node, int dungeonIndex)
		{
			if (node == null)
			{
				return;
			}
			CanControl = node.NodeData != null && node.NodeData.ControlNPC;
			if (!CanControl)
			{
				return;
			}
			CampaignNodeNPCData data = GameRoot.Instance.CampaignNodeNPCDataContainer.GetData(node.NodeData.ChapterIndex, node.NodeData.DungeonIndex);
			if (data == null)
			{
				return;
			}
			NodeNPCData = data;
			Avatar.Show(node.IsVisibleAvatar);
			CampaignNodeNpcAvatarManager.NpcUpdateInfo updateInfo = GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.GetUpdateInfo(dungeonIndex);
			if (updateInfo != null)
			{
				if (updateInfo.NpcState == CampaignNodeNPCData.NPCAvatarState.Appear)
				{
					Avatar.Show(show: false);
				}
			}
			else if (node.IsCompleted)
			{
				Avatar.SetState(CampaignNodeNPCData.NPCAvatarState.Lose, data.LoseMotionValues);
			}
			else
			{
				Avatar.SetState(CampaignNodeNPCData.NPCAvatarState.Alive, data.AliveMotionValues);
			}
		}
	}

	private Mediator _mediator;

	private List<AvatarWrapper> _avatarList = new List<AvatarWrapper>();

	private List<CrowdMediator> _crowdMediators = new List<CrowdMediator>();

	public NpcAvatar GetNPCAvatar(int chapterIndex, int dungeonIndex)
	{
		AvatarWrapper avatarWrapper = _avatarList.Find((AvatarWrapper x) => x.NodeNPCData.ChapterIndex == chapterIndex && x.NodeNPCData.DungeonIndex == dungeonIndex);
		if (avatarWrapper == null)
		{
			VespaDebug.LogError($"CAN'T FIND GetNPCAvatar!!! chapterIndex = {chapterIndex} / dungeonIndex = {dungeonIndex}");
			return null;
		}
		return avatarWrapper.Avatar;
	}

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
	}

	public void Close()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarList); i++)
		{
			_avatarList[i].Avatar.Close();
		}
		_avatarList.Clear();
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_crowdMediators);
	}

	public void Update()
	{
		MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		float extraAlpha = Mathf.Clamp01(1f - instance.MapViewer.GetNodeMenuWeight() * 2f);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarList); i++)
		{
			AvatarWrapper avatarWrapper = _avatarList[i];
			NpcAvatar avatar = avatarWrapper.Avatar;
			NPCData npcData = avatar.NpcData;
			bool show = IsNpcAvatarEnabled(npcData);
			avatar.SetExtraAlpha(extraAlpha);
			avatar.ChainUpdate();
			if (!avatarWrapper.CanControl)
			{
				avatar.Show(show);
			}
		}
	}

	public void ForEachAvatarOfOwner(IAvatarOwner owner, Action<NpcAvatar> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarList); i++)
		{
			AvatarWrapper avatarWrapper = _avatarList[i];
			if (avatarWrapper.Owner == owner)
			{
				Vespa_System_Action_Extension_Method.fire(action, avatarWrapper.Avatar);
			}
		}
	}

	private bool IsNpcAvatarEnabled(NPCData npcData)
	{
		if (npcData == null)
		{
			return false;
		}
		if (!SingletonAuto<ConditionChecker>.instance.Check(npcData.NpcCondition))
		{
			return false;
		}
		return true;
	}

	public NpcAvatar CreateNpcAvatar(IAvatarOwner owner, MapNode node, string npcCode)
	{
		if (node == null)
		{
			return null;
		}
		if (node.NodeData != null && node.NodeData.OpenBackgroundDownloading && GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading())
		{
			return null;
		}
		if (String_Extension_Method.IsNullOrEmpty(npcCode))
		{
			return null;
		}
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(npcCode);
		if (data == null)
		{
			return null;
		}
		int key = data.CreatureIndex;
		if (LobbyManagement.TestGoatWarrior)
		{
			key = 6301;
		}
		CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(key);
		if (data2 == null)
		{
			return null;
		}
		Vector3 position = node.GetPosition();
		int npcAngle = data.NpcAngle;
		int scale = ((data.NpcScale > 0) ? data.NpcScale : 100);
		if (__Extension_Method_ICollection__.COUNT(data.NpcOffset) >= 2)
		{
			position += new Vector3(data.NpcOffset[0], data.NpcOffset[1], 0f);
		}
		int chapterIndex = node.ChapterIndex;
		int linkedDungeonIndex = node.LinkedDungeonIndex;
		bool show = IsNpcAvatarEnabled(data);
		MapComponent mapComponent = _mediator.MapComponent;
		NpcAvatar npcAvatar = new NpcAvatar();
		CrowdMediator crowdMediator = new CrowdMediator();
		AvatarController avatarController = new AvatarController(crowdMediator);
		CrowdMediator.CrowdMediatorInfo crowdMediatorInfo = new CrowdMediator.CrowdMediatorInfo();
		int chapterIndex2 = (crowdMediator.OrginChapterIndex = chapterIndex);
		crowdMediatorInfo.ChapterIndex = chapterIndex2;
		chapterIndex2 = (crowdMediator.OrginChapterIndex = chapterIndex);
		crowdMediatorInfo.DungeonIndex = chapterIndex2;
		crowdMediatorInfo.Avatar = npcAvatar;
		crowdMediatorInfo.AvatarController = avatarController;
		crowdMediatorInfo.MapComponent = mapComponent;
		crowdMediatorInfo.CrowdType = CrowdType.PartyMember;
		crowdMediator.Init(crowdMediatorInfo);
		_crowdMediators.Add(crowdMediator);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, data2.CodeName, (data.CreatureStar == 0) ? 1 : data.CreatureStar);
		if (IsControlAvatar(node))
		{
			npcAvatar.SetRenderFacadeLoadLevel(CreatureStock.ELoadLevel.NodeNPC);
		}
		npcAvatar.Open(avatarController, mapComponent, renderFacadeBuilder, npcCode, node);
		npcAvatar.SetPosition(position);
		npcAvatar.SetDirection(npcAngle);
		npcAvatar.SetScale(scale);
		npcAvatar.Show(show);
		AvatarWrapper avatarWrapper = new AvatarWrapper
		{
			Avatar = npcAvatar,
			Owner = owner
		};
		avatarWrapper.SetControlNPC(node, linkedDungeonIndex);
		_avatarList.Add(avatarWrapper);
		return npcAvatar;
	}

	private bool IsControlAvatar(MapNode node)
	{
		if (node == null)
		{
			return false;
		}
		if (node.NodeData == null || !node.NodeData.ControlNPC)
		{
			return false;
		}
		if (GameRoot.Instance.CampaignNodeNPCDataContainer.GetData(node.NodeData.ChapterIndex, node.NodeData.DungeonIndex) == null)
		{
			return false;
		}
		return true;
	}

	public bool RemoveAvatar(IAvatarOwner owner, int chapterIndex, int dungeonIndex, string npcCode)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarList); i++)
		{
			AvatarWrapper avatarWrapper = _avatarList[i];
			if (avatarWrapper.Owner == owner)
			{
				NpcAvatar avatar = avatarWrapper.Avatar;
				if (avatar.ChapterIndex == chapterIndex && avatar.DungeonIndex == dungeonIndex && string.Equals(avatar.NPCCode, npcCode, StringComparison.InvariantCultureIgnoreCase))
				{
					_avatarList.RemoveAt(i);
					avatar.Close();
					return true;
				}
			}
		}
		return false;
	}

	public void UpdateNPCState(List<CampaignNodeNpcAvatarManager.NpcUpdateInfo> npcInfoList, CampaignNodeNPCData.NPCAvatarState state)
	{
		int i;
		for (i = 0; i < __Extension_Method_ICollection__.COUNT(npcInfoList); i++)
		{
			AvatarWrapper avatarWrapper = _avatarList.Find((AvatarWrapper w) => w.Avatar.GetDungeonIndex() == npcInfoList[i].DungeonIndex);
			if (avatarWrapper != null && avatarWrapper.CanControl && avatarWrapper.NodeNPCData != null && npcInfoList[i].NpcState == state)
			{
				avatarWrapper.Avatar.Show(show: true);
				avatarWrapper.Avatar.SetState(npcInfoList[i].NpcState, avatarWrapper.NodeNPCData.GetMotionValue(npcInfoList[i].NpcState));
			}
		}
	}

	void IAvatarContainer.ForEach(Action<Avatar> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarList); i++)
		{
			Vespa_System_Action_Extension_Method.fire(action, _avatarList[i].Avatar);
		}
	}
}
