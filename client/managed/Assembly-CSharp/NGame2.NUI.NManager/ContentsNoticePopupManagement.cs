using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NCutScene;
using NGame2.NNotify;
using NGame2.NParty;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NEntryMenu;
using NShared;
using NShared.GetContentsNoticePopupRankerList;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class ContentsNoticePopupManagement : BaseManagement<ContentsNoticePopupManagement>, ContentsNoticePopup.IListener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<ContentsNoticePopup>("Resources/Prefab2/Window/MessagePopup/ContentsNoticePopup");
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (GlobalAutoBehaviour<InviteNotifyManager>.isValidInstance && GlobalAutoBehaviour<InviteNotifyManager>.instance.CurrentWindow != null)
		{
			ContentsNoticePopup contentsNoticePopup = GetInstance<ContentsNoticePopup>(openedOnly: true);
			if (contentsNoticePopup != null && contentsNoticePopup.IsOpen())
			{
				contentsNoticePopup.Close();
			}
		}
	}

	public void OpenContentsNodeStartPopup(int chapterIndex, int dungeonIndex)
	{
		IList<ContentsNoticePopupData> allData = GameRoot.Instance.ContentsNoticePopupDataContainer.GetAllData();
		ContentsNoticePopupData popupData = allData.FirstOrDefault((ContentsNoticePopupData x) => x.ChapterIndex == chapterIndex && x.DungeonIndex == dungeonIndex && x.PopupType == ContentsNoticePopupType.GlobalNodeStart);
		if (popupData == null)
		{
			return;
		}
		ContentsNoticeType contentsType = popupData.ContentsType;
		ContentsNoticePopupManager popupManager = SingletonAuto<ContentsNoticePopupManager>.instance;
		List<ContentsNoticePopupInfo> allInfos = popupManager.GetAllInfos();
		ContentsNoticePopupInfo info = allInfos.Find((ContentsNoticePopupInfo x) => x.ContentsType == contentsType);
		if (info == null)
		{
			return;
		}
		SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(MatchArena.MatchSeasonInfo seasonData)
		{
			if (seasonData != null && !popupManager.CheckContentsNoticePopupOpen(info.ContentsType, ContentsNoticePopupType.GlobalNodeStart, info.SeasonIndex) && CheckContentsOpenStatus(info) && CheckContentsNoticeOpenCondition(popupData))
			{
				ContentsNoticePopup contentsNoticePopup = GetInstance<ContentsNoticePopup>();
				if (!(contentsNoticePopup == null))
				{
					contentsNoticePopup.Open(this, popupData, info.SeasonIndex, null, null);
					SetPopupOpenedSeasonIndex(popupData, info.SeasonIndex);
				}
			}
		});
	}

	public void OpenContentsStartPopup()
	{
		if (IsTutorial() || (GlobalAutoBehaviour<InviteNotifyManager>.isValidInstance && GlobalAutoBehaviour<InviteNotifyManager>.instance.CurrentWindow != null))
		{
			return;
		}
		ContentsNoticePopupManager contentsNoticePopupManager = SingletonAuto<ContentsNoticePopupManager>.instance;
		List<ContentsNoticePopupInfo> allInfos = contentsNoticePopupManager.GetAllInfos();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allInfos); i++)
		{
			ContentsNoticePopupInfo info = allInfos[i];
			if (contentsNoticePopupManager.CheckContentsNoticePopupOpen(info.ContentsType, ContentsNoticePopupType.GlobalStart, info.SeasonIndex) || !CheckContentsOpenStatus(info))
			{
				continue;
			}
			ContentsNoticePopupData popupData = GameRoot.Instance.ContentsNoticePopupDataContainer.GetData(info, ContentsNoticePopupType.GlobalStart);
			if (popupData != null && CheckContentsNoticeOpenCondition(popupData))
			{
				ContentsNoticePopup popupWindow = GetInstance<ContentsNoticePopup>();
				SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(popupWindow, delegate(ContentsNoticePopup window)
				{
					SetPopupOpenedSeasonIndex(popupData, info.SeasonIndex);
					window.Open(this, popupData, info.SeasonIndex, null, null);
				}, PopupQueue.PopupFlag.HideOnTopmostMenuOpen);
				break;
			}
		}
	}

	public void OpenContentsResultPopup(ContentsNoticeType contentsType)
	{
		if (IsTutorial() || (GlobalAutoBehaviour<InviteNotifyManager>.isValidInstance && GlobalAutoBehaviour<InviteNotifyManager>.instance.CurrentWindow != null))
		{
			return;
		}
		if (contentsType == ContentsNoticeType.BanPick)
		{
			MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
			if (matchSeasonInfo == null || matchSeasonInfo.MatchSeasonType == MatchSeasonType.Regular)
			{
				return;
			}
		}
		ContentsNoticePopupManager contentsNoticePopupManager = SingletonAuto<ContentsNoticePopupManager>.instance;
		int prevSeasonIndex = contentsNoticePopupManager.GetPreviousSeasonIndex(contentsType);
		if (prevSeasonIndex <= 0)
		{
			return;
		}
		NShared.GetContentsNoticePopupRankerList.Request request = new NShared.GetContentsNoticePopupRankerList.Request();
		request.ContentsType = contentsType;
		request.SeasonIndex = prevSeasonIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetContentsNoticePopupRankerList.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
			{
				ContentsNoticePopupRankerInfo[] rankerInfos = response.RankerInfos;
				ContentsNoticePopupServerTotalRankingInfo[] serverRankInfos = response.ServerRankInfos;
				ContentsNoticePopupInfo info = new ContentsNoticePopupInfo
				{
					ContentsType = contentsType,
					SeasonIndex = prevSeasonIndex,
					ChapterIndex = response.ChapterIndex,
					DungeonIndex = response.DungeonIndex
				};
				OpenContentsNoticePopup(info, ContentsNoticePopupType.GlobalEnd, rankerInfos, serverRankInfos, contentsType);
			}
		});
	}

	private void OpenContentsNoticePopup(ContentsNoticePopupInfo info, ContentsNoticePopupType popupType, ContentsNoticePopupRankerInfo[] rankerInfos, ContentsNoticePopupServerTotalRankingInfo[] serverRankInfos, ContentsNoticeType contentsNoticeType)
	{
		ContentsNoticePopupData data = GameRoot.Instance.ContentsNoticePopupDataContainer.GetData(info, popupType);
		if (data != null && CheckContentsNoticeOpenCondition(data))
		{
			if (popupType == ContentsNoticePopupType.GlobalEnd && !CheckAvailableRankingData(rankerInfos))
			{
				VespaDebug.LogError("rankerInfos or serverRankInfos wrong");
				return;
			}
			SetPopupOpenedSeasonIndex(data, info.SeasonIndex);
			GetInstance<ContentsNoticePopup>().Open(this, data, info.SeasonIndex, rankerInfos, serverRankInfos);
		}
	}

	private void SetPopupOpenedSeasonIndex(ContentsNoticePopupData popupData, int seasonIndex)
	{
		SingletonAuto<ContentsNoticePopupManager>.instance.SetContentsNoticePopupOpenSeasonIndex(popupData.ContentsType, popupData.PopupType, seasonIndex);
	}

	private bool CheckContentsOpenStatus(ContentsNoticePopupInfo popupInfo)
	{
		if (popupInfo.ContentsType == ContentsNoticeType.WorldBoss)
		{
			WorldBossInfo worldBossInfo = SingletonUpdater<WorldBoss>.instance.WorldBossInfo;
			if (worldBossInfo == null)
			{
				return false;
			}
			if (worldBossInfo.Status == WorldBossStatus.Ended)
			{
				return false;
			}
		}
		else if (popupInfo.ContentsType == ContentsNoticeType.BanPick)
		{
			if (GetMatchBanPickSeasonType(popupInfo.SeasonIndex) == MatchSeasonType.Free)
			{
				return false;
			}
		}
		else if (popupInfo.ContentsType == ContentsNoticeType.GuildSuppress)
		{
			GuildSuppressSessionData guildSuppressSessionData = RestoredPortal.ConquestSession(GameRoot.Instance.GuildSuppressSessionDataContainer, popupInfo.SessionIndex);
			if (guildSuppressSessionData == null)
			{
				return false;
			}
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalApplyStart, out var o_dateTime) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(guildSuppressSessionData.GlobalBattleEnd, out var o_dateTime2))
			{
				return false;
			}
			DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
			if (currentServerUTCTime < o_dateTime || currentServerUTCTime > o_dateTime2)
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckContentsNoticeOpenCondition(ContentsNoticePopupData popupData)
	{
		if (popupData == null)
		{
			return false;
		}
		if (!NGame2.NUI.Common.CheckContentsOpenCondition(popupData.OpenCondition))
		{
			return false;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(popupData.TutorialIndex))
		{
			return false;
		}
		return true;
	}

	private bool CheckAvailableRankingData(ContentsNoticePopupRankerInfo[] rankerInfos)
	{
		if (__Extension_Method_ICollection__.COUNT(rankerInfos) == 0)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(Array.FindAll(rankerInfos, (ContentsNoticePopupRankerInfo info) => info.Score == 0)) == __Extension_Method_ICollection__.COUNT(rankerInfos))
		{
			return false;
		}
		return true;
	}

	private bool IsTutorial()
	{
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return true;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return true;
			}
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			return true;
		}
		return false;
	}

	private MatchSeasonType GetMatchBanPickSeasonType(int seasonIndex)
	{
		IList<MatchSeasonData> allData = GameRoot.Instance.MatchBanPickSeasonDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return MatchSeasonType.Free;
		}
		return allData.FirstOrDefault((MatchSeasonData v) => v.SeasonIndex == seasonIndex)?.MatchSeasonType ?? MatchSeasonType.Free;
	}

	void ContentsNoticePopup.IListener.OnClickMovoToContents(ContentsNoticePopupData popupData)
	{
		new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate(ContentsNoticePopup window)
		{
			window.Close();
		})).ADDTASK(base.TaskFactory.Run(delegate
		{
			float sec = ((SceneAutoBehaviour<EntryMenuManagement>.isValidInstance && SceneAutoBehaviour<EntryMenuManagement>.instance.IsOpened<NGame2.NUI.NWindow.EntryMenu>()) ? 0.1f : 0f);
			VespaCoroutine.Start(CoMoveToContents(popupData, sec));
		})).WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	private IEnumerator CoMoveToContents(ContentsNoticePopupData popupData, float sec)
	{
		yield return new WaitForSeconds(sec);
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}
		string actionName = ArgParser.GetValueString(popupData.OpenAction, 0);
		if (String_Extension_Method.IsNullOrEmpty(actionName))
		{
			yield return null;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(actionName);
		if (action == null)
		{
			yield return null;
		}
		List<object> list = new List<object>();
		string[] openAction = popupData.OpenAction;
		foreach (string item in openAction)
		{
			list.Add(item);
		}
		action.Run(null, null, list);
	}

	void ContentsNoticePopup.IListener.OnClickStartPopupClose()
	{
		GetInstance<ContentsNoticePopup>(openedOnly: true).Close();
		OpenContentsStartPopup();
	}

	void ContentsNoticePopup.IListener.OnClickMoveToHonoraryChamber(ContentsNoticeType contentType)
	{
		LobbyManagement lobbyManagement = SceneAutoBehaviour<LobbyManagement>.instance;
		if (!(lobbyManagement == null))
		{
			ContentsNoticeTypeAttribute contentsNoticeTypeAttribute = EnumAttributeContainer<ContentsNoticeType, ContentsNoticeTypeAttribute>.Get(contentType);
			if (contentsNoticeTypeAttribute.RecordsOfHonorContentType != RecordsOfHonorContentType.None)
			{
				lobbyManagement.OpenHonoraryChamber(contentsNoticeTypeAttribute.RecordsOfHonorContentType, HonoraryChamber.ServerType.World);
			}
		}
	}
}
