using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public abstract class BaseEntryMenuReturnState : LobbyScene2.IReturnState
{
	public enum SaveKey
	{
		RewardData,
		LeftTeamKillCount
	}

	protected Dictionary<SaveKey, object> _saveDataDic;

	string LobbyScene2.IReturnState.BgmCode
	{
		get
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data == null)
			{
				return string.Empty;
			}
			if (!String_Extension_Method.IsNullOrEmpty(data.EntryNPCCode))
			{
				EntryMenuSettingData dataByMenuType = GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(data.EntryNPCCode);
				if (dataByMenuType == null || String_Extension_Method.IsNullOrEmpty(dataByMenuType.EventBgmCode))
				{
					return string.Empty;
				}
				if (!String_Extension_Method.IsNullOrEmpty(dataByMenuType.EventBgmCode))
				{
					return dataByMenuType.EventBgmCode;
				}
			}
			return string.Empty;
		}
	}

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null || String_Extension_Method.IsNullOrEmpty(data.EntryNPCCode))
		{
			Execute(info, onEnd);
			return;
		}
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(chapterIndex, dungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: false, delegate
		{
			Execute(info, onEnd);
		});
	}

	public abstract void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd);

	public virtual void AddSaveData(SaveKey key, object value)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_saveDataDic) && _saveDataDic.ContainsKey(key))
		{
			_saveDataDic[key] = value;
		}
	}
}
