using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class ChapterFlowManager : ChapterAvatarManager.IListener
{
	public interface IListener
	{
		void ChangeStatus(StatusType type);
	}

	public enum StatusType
	{
		ChangeToMyAvatar,
		EndAllLine
	}

	public enum ConditionKeyType
	{
		OpenNode,
		CompleteDungeon,
		CompleteSubStory
	}

	public class ChapterFlowInfo
	{
		public int ChapterIndex { get; set; }

		public Mediator MyAvatarMediator { get; set; }

		public CrowdManager CrowdManager { get; set; }
	}

	private const string LocalAccountDateKey = "_08071531";

	private const string PairStatusIdleKey = "Idle";

	private IListener _iListener;

	private ChapterFlowInfo _chapterFlowInfo;

	private ChapterAvatarManager _chapterAvatarManager;

	private LocalAccountInt _localAccountLineIndex;

	private LocalAccountBool _localAccountPairCenterMessage;

	private LocalAccountBool _localAccountPairEffect;

	private int NextLineIndex
	{
		get
		{
			if (_chapterFlowInfo == null)
			{
				return -1;
			}
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(_chapterFlowInfo.ChapterIndex);
			if (currentCampaignMultiAvatarData == null)
			{
				return -1;
			}
			List<CampaignMultiAvatarData> dataList = GameRoot.Instance.CampaignMultiAvatarDataContainer.GetDataList(_chapterFlowInfo.ChapterIndex);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(dataList))
			{
				return -1;
			}
			CampaignMultiAvatarData campaignMultiAvatarData = null;
			for (int i = 0; i < dataList.Count; i++)
			{
				campaignMultiAvatarData = dataList[i];
				if (campaignMultiAvatarData != null && campaignMultiAvatarData.LineIndex > currentCampaignMultiAvatarData.LineIndex)
				{
					return campaignMultiAvatarData.LineIndex;
				}
			}
			return -1;
		}
	}

	public ChapterFlowManager(IListener iListener, int chapterIndex)
	{
		_iListener = iListener;
		_localAccountLineIndex = new LocalAccountInt(1, GetLocalAccountKey(chapterIndex));
		CreateChapterAvatarManager(chapterIndex);
	}

	public void Init(ChapterFlowInfo info)
	{
		_chapterFlowInfo = info;
		if (info != null)
		{
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(info.ChapterIndex);
			if (currentCampaignMultiAvatarData == null)
			{
				EndAllLine(info.ChapterIndex);
				return;
			}
			int startDungeonIndex = GetStartDungeonIndex(currentCampaignMultiAvatarData, dungeonIndex);
			SetChapterAvatarManager(currentCampaignMultiAvatarData.CostumeIndex, startDungeonIndex);
			CreatePartyMemberAvatar(startDungeonIndex);
			ShowPairIndicator(info.ChapterIndex);
		}
	}

	public void Close()
	{
		_chapterFlowInfo = null;
		_iListener = null;
		CloseChapterAvatarManager();
	}

	private void CreateChapterAvatarManager(int chapterIndex)
	{
		if (_chapterAvatarManager == null)
		{
			_chapterAvatarManager = new ChapterAvatarManager(this);
		}
		if (!_chapterAvatarManager.CheckUseSpecificAvatar(chapterIndex))
		{
			_chapterAvatarManager.ChangeToMyAvatar();
			CloseChapterAvatarManager();
		}
	}

	private void SetChapterAvatarManager(int costumeIndex, int dungeonIndex)
	{
		if (_chapterFlowInfo != null)
		{
			_chapterAvatarManager.Init(new ChapterAvatarManager.ChapterAvatarInfo
			{
				ChapterIndex = _chapterFlowInfo.ChapterIndex,
				DungeonIndex = dungeonIndex,
				CostumeIndex = costumeIndex,
				MyAvatarMediator = _chapterFlowInfo.MyAvatarMediator
			});
		}
	}

	private void CreatePartyMemberAvatar(int dungeonIndex)
	{
		if (_chapterFlowInfo == null || _chapterFlowInfo.CrowdManager == null || _chapterAvatarManager == null)
		{
			return;
		}
		CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(_chapterFlowInfo.ChapterIndex);
		if (currentCampaignMultiAvatarData != null && currentCampaignMultiAvatarData.PairCostumeIndex > 0)
		{
			int pairStartDungeonIndex = GetPairStartDungeonIndex(currentCampaignMultiAvatarData, dungeonIndex);
			CrowdManager.AvatarInfo info = new CrowdManager.AvatarInfo
			{
				CostumeIndex = currentCampaignMultiAvatarData.PairCostumeIndex,
				ChapterIndex = _chapterFlowInfo.ChapterIndex,
				DungeonIndex = pairStartDungeonIndex
			};
			if (currentCampaignMultiAvatarData.PairStatus == "Idle")
			{
				_chapterFlowInfo.CrowdManager.CreateCrowdAvatar(info);
			}
			else
			{
				_chapterFlowInfo.CrowdManager.CreatePartyMemberAvatar(info);
			}
		}
	}

	private void CloseChapterAvatarManager()
	{
		if (_chapterAvatarManager != null)
		{
			_chapterAvatarManager.Close();
			_chapterAvatarManager = null;
		}
	}

	private void SaveLineIndex(int lineIndex)
	{
		if (_localAccountLineIndex != null)
		{
			_localAccountLineIndex.Value = lineIndex;
			SetLocalAcccountPairIndicator(lineIndex);
		}
	}

	private void SetLocalAcccountPairIndicator(int lineIndex)
	{
		if (_chapterFlowInfo != null)
		{
			_localAccountPairCenterMessage = new LocalAccountBool(i_default: false, GetLocalAccountKeyMessage(lineIndex));
			_localAccountPairEffect = new LocalAccountBool(i_default: false, GetLocalAccountKeyEffect(lineIndex));
		}
	}

	public void ChangeToMyAvatar()
	{
		if (_iListener != null)
		{
			_iListener.ChangeStatus(StatusType.ChangeToMyAvatar);
		}
	}

	private int GetStartDungeonIndex(CampaignMultiAvatarData data, int dungeonIndex)
	{
		if (data == null)
		{
			return dungeonIndex;
		}
		if (data.StartNode <= 0)
		{
			return dungeonIndex;
		}
		if (_chapterFlowInfo == null)
		{
			return data.StartNode;
		}
		CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(_chapterFlowInfo.ChapterIndex, dungeonIndex);
		if (data2 == null)
		{
			return data.StartNode;
		}
		if (data2.Function == NodeFunctionType.MoveChapter)
		{
			return data.StartNode;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_chapterFlowInfo.ChapterIndex, data.StartNode))
		{
			return dungeonIndex;
		}
		return data.StartNode;
	}

	private int GetPairStartDungeonIndex(CampaignMultiAvatarData data, int dungeonIndex)
	{
		if (data == null)
		{
			return dungeonIndex;
		}
		if (data.PairStartNode <= 0)
		{
			return dungeonIndex;
		}
		if (data.PairStatus == "Idle")
		{
			return data.PairStartNode;
		}
		if (_chapterFlowInfo == null)
		{
			return dungeonIndex;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(_chapterFlowInfo.ChapterIndex, data.PairStartNode))
		{
			return dungeonIndex;
		}
		return data.PairStartNode;
	}

	private CampaignMultiAvatarData GetCurrentCampaignMultiAvatarData(int chapterIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ClearedChapter(chapterIndex))
		{
			return null;
		}
		List<CampaignMultiAvatarData> dataList = GameRoot.Instance.CampaignMultiAvatarDataContainer.GetDataList(chapterIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(dataList))
		{
			return null;
		}
		_ = string.Empty;
		CampaignMultiAvatarData campaignMultiAvatarData = null;
		for (int i = 0; i < dataList.Count; i++)
		{
			campaignMultiAvatarData = dataList[i];
			if (campaignMultiAvatarData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(campaignMultiAvatarData.NextStartCondition) && campaignMultiAvatarData.NextStartCondition.Length >= 1)
			{
				_ = campaignMultiAvatarData.NextStartCondition[0];
				List<string> list = new List<string>();
				list.Clear();
				list.AddRange(campaignMultiAvatarData.NextStartCondition.ToList());
				if (list[0].Equals(ConditionKeyType.OpenNode.ToString()))
				{
					list[0] = ConditionKeyType.CompleteDungeon.ToString();
					list.Add(true.ToString());
				}
				if (!SingletonAuto<ConditionChecker>.instance.Check(list.ToArray()))
				{
					break;
				}
				campaignMultiAvatarData = null;
			}
		}
		if (campaignMultiAvatarData == null)
		{
			return null;
		}
		int saveLineIndex = GetSaveLineIndex();
		if (saveLineIndex > campaignMultiAvatarData.LineIndex)
		{
			campaignMultiAvatarData = GameRoot.Instance.CampaignMultiAvatarDataContainer.GetData(chapterIndex, saveLineIndex);
		}
		SaveLineIndex(campaignMultiAvatarData.LineIndex);
		return campaignMultiAvatarData;
	}

	private int GetSaveLineIndex()
	{
		if (_localAccountLineIndex == null)
		{
			return -1;
		}
		return _localAccountLineIndex.Value;
	}

	public bool CheckPossibleFlowChapter(int chapterIndex)
	{
		return GetCurrentCampaignMultiAvatarData(chapterIndex) != null;
	}

	private bool CheckConditionOpenNode(int chapterIndex, int dungeonIndex)
	{
		if (_chapterFlowInfo == null)
		{
			return false;
		}
		CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(_chapterFlowInfo.ChapterIndex);
		if (currentCampaignMultiAvatarData == null)
		{
			return false;
		}
		if (!SingletonAuto<ConditionChecker>.instance.Check(currentCampaignMultiAvatarData.NextStartCondition))
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(currentCampaignMultiAvatarData.NextStartCondition) || currentCampaignMultiAvatarData.NextStartCondition.Length < 3)
		{
			return false;
		}
		if (!currentCampaignMultiAvatarData.NextStartCondition[0].Equals(ConditionKeyType.OpenNode.ToString()))
		{
			return false;
		}
		if (!int.TryParse(currentCampaignMultiAvatarData.NextStartCondition[1], out var result) || !int.TryParse(currentCampaignMultiAvatarData.NextStartCondition[2], out var result2))
		{
			return false;
		}
		if (chapterIndex == result)
		{
			return dungeonIndex == result2;
		}
		return false;
	}

	private string GetLocalAccountKey(int chapterIndex)
	{
		return typeof(ChapterFlowManager).Name + "_" + chapterIndex + "_08071531";
	}

	private string GetLocalAccountKeyMessage(int lineIndex)
	{
		string text = "_Message_" + lineIndex;
		if (_chapterFlowInfo == null)
		{
			return text;
		}
		return GetLocalAccountKey(_chapterFlowInfo.ChapterIndex) + text;
	}

	private string GetLocalAccountKeyEffect(int lineIndex)
	{
		string text = "_Effect_" + lineIndex;
		if (_chapterFlowInfo == null)
		{
			return text;
		}
		return GetLocalAccountKey(_chapterFlowInfo.ChapterIndex) + text;
	}

	public void ProcessChapterFlow(int chapterIndex, int dungeonIndex, ConditionKeyType type)
	{
		switch (type)
		{
		case ConditionKeyType.CompleteDungeon:
			MoveStartNode(chapterIndex, dungeonIndex);
			break;
		case ConditionKeyType.OpenNode:
			if (CheckConditionOpenNode(chapterIndex, dungeonIndex))
			{
				SaveLineIndex(NextLineIndex);
				ShowPairIndicator(chapterIndex);
				MovePartyMemberAvatarToMainAvatar(dungeonIndex);
			}
			break;
		case ConditionKeyType.CompleteSubStory:
		{
			if (_chapterFlowInfo == null)
			{
				return;
			}
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(_chapterFlowInfo.ChapterIndex);
			if (currentCampaignMultiAvatarData != null)
			{
				int startDungeonIndex = GetStartDungeonIndex(currentCampaignMultiAvatarData, dungeonIndex);
				SetChapterAvatarManager(currentCampaignMultiAvatarData.CostumeIndex, startDungeonIndex);
			}
			break;
		}
		}
		EndAllLine(chapterIndex);
	}

	private void ShowPairIndicator(int chapterIndex)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddAction(delegate
		{
			VespaCoroutine.Start(CoShowPairIndicator(chapterIndex));
		}, null, PopupQueue.PopupFlag.HideOnTopmostMenuOpen, "PairIndicator");
	}

	private IEnumerator CoShowPairIndicator(int chapterIndex)
	{
		yield return null;
		ShowCenterMessagePopupByPair(chapterIndex);
		ShowEffectByPair(chapterIndex);
	}

	private void ShowCenterMessagePopupByPair(int chapterIndex)
	{
		if (_localAccountPairCenterMessage == null || !_localAccountPairCenterMessage.Value)
		{
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(chapterIndex);
			if (currentCampaignMultiAvatarData != null && !String_Extension_Method.IsNullOrEmpty(currentCampaignMultiAvatarData.CenterPopupKey))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect(currentCampaignMultiAvatarData.CenterPopupKey));
				_localAccountPairCenterMessage.Value = true;
			}
		}
	}

	private void ShowEffectByPair(int chapterIndex)
	{
		CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(chapterIndex);
		if (currentCampaignMultiAvatarData == null)
		{
			return;
		}
		PairEffectType pairEffectType = currentCampaignMultiAvatarData.PairEffectType;
		Action completeAction = null;
		switch (pairEffectType)
		{
		default:
			return;
		case PairEffectType.StartPair:
			if (_localAccountPairEffect != null && _localAccountPairEffect.Value)
			{
				pairEffectType = PairEffectType.LoopPair;
				break;
			}
			VespaCoroutine.Start(CoPlayPairEffect(PairEffectType.LoopPair, 0.4f));
			if (_localAccountPairEffect != null)
			{
				_localAccountPairEffect.Value = true;
			}
			break;
		case PairEffectType.EndPair:
			if (_localAccountPairEffect != null && _localAccountPairEffect.Value)
			{
				return;
			}
			if (_localAccountPairEffect != null)
			{
				_localAccountPairEffect.Value = true;
			}
			break;
		}
		if (_chapterAvatarManager != null)
		{
			_chapterAvatarManager.PlayPairEffect(pairEffectType, completeAction);
		}
	}

	private IEnumerator CoPlayPairEffect(PairEffectType pairEffectType, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (_chapterAvatarManager != null)
		{
			_chapterAvatarManager.PlayPairEffect(pairEffectType, null);
		}
	}

	public bool CheckEndAllLine(int chapterIndex)
	{
		if (GetCurrentCampaignMultiAvatarData(chapterIndex) == null)
		{
			return GetSaveLineIndex() > 0;
		}
		return false;
	}

	public bool CheckMoveStartNode(int chapterIndex, int dungeonIndex)
	{
		CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(chapterIndex);
		if (currentCampaignMultiAvatarData == null)
		{
			return false;
		}
		if (currentCampaignMultiAvatarData.StartNode <= 0)
		{
			return false;
		}
		if (dungeonIndex == currentCampaignMultiAvatarData.StartNode)
		{
			return false;
		}
		return true;
	}

	private void MoveStartNode(int chapterIndex, int dungeonIndex)
	{
		if (CheckMoveStartNode(chapterIndex, dungeonIndex))
		{
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(chapterIndex);
			if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, currentCampaignMultiAvatarData.StartNode))
			{
				_chapterAvatarManager.WarpTo(currentCampaignMultiAvatarData.StartNode);
			}
		}
	}

	private void MovePartyMemberAvatarToMainAvatar(int targetDungeonIndex)
	{
		if (_chapterFlowInfo != null)
		{
			CampaignMultiAvatarData currentCampaignMultiAvatarData = GetCurrentCampaignMultiAvatarData(_chapterFlowInfo.ChapterIndex);
			if (currentCampaignMultiAvatarData != null && currentCampaignMultiAvatarData.PairCostumeIndex > 0 && !(currentCampaignMultiAvatarData.PairStatus == "Idle") && _chapterFlowInfo.CrowdManager != null)
			{
				_chapterFlowInfo.CrowdManager.ChangeCrowdToPartyMember(currentCampaignMultiAvatarData.PairCostumeIndex);
				_chapterFlowInfo.CrowdManager.PartyMemberAvatarMoveToDungeon(targetDungeonIndex);
			}
		}
	}

	private void EndAllLine(int chapterIndex)
	{
		if (CheckEndAllLine(chapterIndex))
		{
			ChangeToMyAvatar();
			if (_iListener != null)
			{
				_iListener.ChangeStatus(StatusType.EndAllLine);
			}
		}
	}

	void ChapterAvatarManager.IListener.ChangeToMyAvatar()
	{
		ChangeToMyAvatar();
	}

	void ChapterAvatarManager.IListener.RemovePartyMemberAvatars()
	{
		if (_chapterFlowInfo != null && _chapterFlowInfo.CrowdManager != null)
		{
			_chapterFlowInfo.CrowdManager.RemovePartyMemberAvatars();
		}
	}

	void ChapterAvatarManager.IListener.RemoveAllCrowdAvatar()
	{
		if (_chapterFlowInfo != null && _chapterFlowInfo.CrowdManager != null)
		{
			_chapterFlowInfo.CrowdManager.Close();
		}
	}
}
