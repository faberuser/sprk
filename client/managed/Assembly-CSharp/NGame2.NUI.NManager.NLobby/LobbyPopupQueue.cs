using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NCutScene;
using NGame2.NQuest;
using NGame2.NScene;
using NGame2.NSubStory;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NAttendance;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NNotice;
using NGame2.NUI3.NAttendance;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NLobby;

public class LobbyPopupQueue : SceneUpdateBehaviour<LobbyPopupQueue>
{
	private PopupQueue _popupQueue = new PopupQueue();

	private static HashSet<int> _displayedLobbyPopupList = new HashSet<int>();

	private bool _opened;

	private static bool IsFirstOpen = true;

	public bool Empty()
	{
		return _popupQueue.Empty();
	}

	public bool IsProgressing()
	{
		return _popupQueue.IsProgressing();
	}

	public void Open()
	{
		_opened = true;
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, Func<bool> canOpenFunc, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None, string groupName = "") where T : BaseWindow
	{
		if (!(popupWindow == null))
		{
			_popupQueue.AddPopup(popupWindow, openAction, canOpenFunc, flag, groupName);
		}
	}

	public void AddPopup<T>(T popupWindow, Action<T> openAction, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None, string groupName = "") where T : BaseWindow
	{
		AddPopup(popupWindow, openAction, null, flag, groupName);
	}

	public void OpenReservedPopup(string popupName, Action onEnd)
	{
		_popupQueue.OpenReservedPopup(popupName, onEnd);
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdatePopup();
	}

	public void UpdatePopup()
	{
		if (_opened && !LobbyPopupDisabler.IsDisabled() && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && !AutoNextBattle.IsEnabled() && !AutoRepeatBattle.IsEnabled() && (!SceneAutoBehaviour<MatchManagement>.isValidInstance || !SceneAutoBehaviour<MatchManagement>.instance.IsOpenedBanPickWindow()) && !GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.IsFocusing() && (!SceneAutoBehaviour<CutSceneManager>.isValidInstance || !SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying))
		{
			_popupQueue.UpdatePopup();
		}
	}

	public void ReserveAttendancePopup_on_EnterLobby()
	{
		foreach (AttendanceKeywordGroup attendanceGroup in AttendanceFacade.GetGroupList_ReservePopup_on_EnterLobby())
		{
			AttendanceWindow attendanceWindow = SingletonAuto<AttendanceManager>.instance.GetAttendanceWindow(attendanceGroup);
			AddPopup(attendanceWindow, delegate(AttendanceWindow window)
			{
				window.OpenAttendance(attendanceGroup, openedFromReservePopup: true);
			}, PopupQueue.PopupFlag.DefaultPopupFlag);
		}
	}

	public void ReopenAttendancePopup(AttendanceKeywordGroup attendanceGroup, bool isShowPurchaseButton = true)
	{
		if (!attendanceGroup.IsValid_AttendanceData())
		{
			return;
		}
		AttendanceWindow attendanceWindow = SingletonAuto<AttendanceManager>.instance.GetAttendanceWindow(attendanceGroup);
		AddPopup(attendanceWindow, delegate(AttendanceWindow window)
		{
			window.OpenAttendance(attendanceGroup, openedFromReservePopup: false);
			if (!isShowPurchaseButton)
			{
				window.ShowPurchaseButton(isShowPurchaseButton);
			}
		});
	}

	public void ReserveAccumulateLoginPopup()
	{
		if (AccumulateLogin.RewardAble() && !AutoNextBattle.IsEnabled())
		{
			AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<AccumulateLogin>(), delegate(AccumulateLogin window)
			{
				window.Open(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.LoginDailyCount, is_in_AttendanceIntegration: false);
			}, PopupQueue.PopupFlag.DefaultPopupFlag);
		}
	}

	public void ReserveUnlockCostumePopup()
	{
		if (SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap && GlobalAutoBehaviour<CostumeOpenChecker>.instance.ExistUnlockCostume())
		{
			CostumeData costumeData = GlobalAutoBehaviour<CostumeOpenChecker>.instance.DequeueUnlockCostumeInQ();
			AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<CostumeOpenPopup>(), delegate(CostumeOpenPopup window)
			{
				window.Open(costumeData, null);
			}, CostumeOpenPopup.CanOpen);
		}
	}

	public void ReserveLobbyNoticePopup(LobbyNotice notice)
	{
		if (!Helper.CheckRestriction("LobbyNoticePopupOpenCondition", cacheResult: true) || notice == null)
		{
			return;
		}
		int noticeIndex = notice.Index;
		if (!_displayedLobbyPopupList.Contains(noticeIndex))
		{
			PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.DefaultPopupFlag;
			AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyNoticePopup>(), delegate(LobbyNoticePopup window)
			{
				_displayedLobbyPopupList.Add(noticeIndex);
				window.Open(notice);
			}, flag, "LobbyNoticePopup");
		}
	}

	public void ReserveDungeonOpenPopUp()
	{
		CampaignNodeData dungeonNode = SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen();
		if (dungeonNode != null)
		{
			AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<ContentsOpenPopup>(), delegate(ContentsOpenPopup window)
			{
				window.Open(dungeonNode);
			});
		}
	}

	public void ReserveMenuOpenPopup()
	{
		List<ERightMenuButton> list = SingletonAuto<ContentsOpenChecker>.instance.CheckMenusOpen();
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return;
		}
		foreach (ERightMenuButton item in list)
		{
			BottomButtonData data = GameRoot.Instance.BottomButtonDataContainer.GetData(item);
			if (data == null)
			{
				break;
			}
			string code = data.ContentsOpenCode;
			if (String_Extension_Method.IsNullOrEmpty(code))
			{
				break;
			}
			if (data.Type != ERightMenuButton.ClassBuff)
			{
				AddPopup(SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<ContentsOpenPopup>(), delegate(ContentsOpenPopup window)
				{
					window.Open(code);
				}, PopupQueue.PopupFlag.DefaultPopupFlag);
			}
		}
	}

	public void ReservedMainQuestClearPopup()
	{
		SingletonUpdater<QuestManager>.instance.OpenClearMainQuestConfirmNotiPopup(ReservedMainQuestClearPopup);
	}

	private void ReservedMainQuestClearPopup(int step)
	{
	}

	public void ReservedContentsOpenConditionPopup()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		NGame2.NAccount.UserQuestManager questManager = GlobalAutoBehaviour<AccountManager>.instance.QuestManager;
		List<SubQuestInfo> clearSubQuestInfoList = questManager.ClearSubQuestInfoList;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(clearSubQuestInfoList); i++)
		{
			SubQuestInfo subQuestInfo = clearSubQuestInfoList[i];
			if (subQuestInfo == null)
			{
				continue;
			}
			List<ContentsOpenConditionData> openContentsListBySubQuestIndex = GameRoot.Instance.ContentsOpenConditionDataContainer.GetOpenContentsListBySubQuestIndex(subQuestInfo.SubQuestIndex);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(openContentsListBySubQuestIndex); j++)
			{
				ContentsOpenConditionData contentsOpenConditionData = openContentsListBySubQuestIndex[j];
				if (contentsOpenConditionData != null && contentsOpenConditionData.OpenText)
				{
					ReservedContentsOpenConditionPopup(contentsOpenConditionData);
				}
			}
		}
		questManager.ClearMainQuestByOpenContentsList.Clear();
	}

	public void ReservedContentsOpenConditionPopup(ContentsOpenConditionData data)
	{
		if (!RestoredPortal.ReserveContentsUnlockNotice(data))
		{
			return;
		}
		ContentsOpenConditionData data2 = data;
		AddAction(delegate
		{
			ContentsOpenPopup contentsOpenPopup = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<ContentsOpenPopup>();
			if (contentsOpenPopup != null)
			{
				contentsOpenPopup.Open(data2.OpenTextResorce, LobbyManagement.LocalizeContentsName(data2));
			}
		}, () => SceneAutoBehaviour<LobbyManagement>.instance.IsOpened<ContentsOpenPopup>(), PopupQueue.PopupFlag.HideOnTopmostMenuOpen, "ContentsOpenCondition");
	}

	public void ReserveSubQuestClearPopup(bool clearQuestList = true)
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.QuestManager.CanOpenSubQuestNotiPopup())
		{
			Vespa_Generic_ICollection_Extension_Method.CLEAR(GlobalAutoBehaviour<AccountManager>.instance.QuestManager.ClearSubQuestInfoList);
			return;
		}
		NGame2.NAccount.UserQuestManager questManager = GlobalAutoBehaviour<AccountManager>.instance.QuestManager;
		List<SubQuestInfo> clearSubQuestInfoList = questManager.ClearSubQuestInfoList;
		for (int i = 0; i < clearSubQuestInfoList.Count; i++)
		{
			SubQuestInfo subQuestInfo = clearSubQuestInfoList[i];
			if (subQuestInfo != null)
			{
				ReserveSubQuestClearPopup(subQuestInfo);
			}
		}
		if (clearQuestList)
		{
			questManager.ClearSubQuestInfoList.Clear();
		}
	}

	public void ReserveMainSubQuestOpenPopup()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.instance.QuestManager.CanOpenSubQuestNotiPopup())
		{
			Vespa_Generic_ICollection_Extension_Method.CLEAR(GlobalAutoBehaviour<AccountManager>.instance.QuestManager.ClearMainQuestInfoList);
			return;
		}
		NGame2.NAccount.UserQuestManager questManager = GlobalAutoBehaviour<AccountManager>.instance.QuestManager;
		List<SubQuestInfo> clearMainQuestInfoList = questManager.ClearMainQuestInfoList;
		for (int i = 0; i < clearMainQuestInfoList.Count; i++)
		{
			SubQuestInfo subQuestInfo = clearMainQuestInfoList[i];
			if (subQuestInfo != null)
			{
				MissionVisualData nextVisualData = GameRoot.Instance.MissionVisualDataContainer.GetNextVisualData(subQuestInfo.SubQuestIndex);
				if (nextVisualData != null)
				{
					SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(nextVisualData.QuestIndex);
					ReserveSubQuestOpenPopup(subQuestData);
				}
			}
		}
		questManager.ClearMainQuestInfoList.Clear();
	}

	public void ReserveSubQuestClearPopup(SubQuestInfo info)
	{
	}

	public void ReserveSubQuestOpenPopup(SubQuestData data)
	{
	}

	public void ReservePurchaseMarketingPopup()
	{
		if (Helper.CheckRestriction("PurchaseMarketingOpenCondition", cacheResult: true))
		{
			AddAction(delegate
			{
				GlobalAutoBehaviour<PurchaseMarketingManager>.instance.OpenPurchaseMarketingPopup(OpenConditionType.EnterLobby);
			}, () => PurchaseMarketingPopup.IsValidInstance, PopupQueue.PopupFlag.DefaultPopupFlag, "PurchaseMarketing");
		}
	}

	public void ReserveContentsNoticePopup()
	{
		AddAction(delegate
		{
			SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsStartPopup();
		}, () => !GlobalAutoBehaviour<GlobalManagement>.isValidInstance || !GlobalAutoBehaviour<GlobalManagement>.instance.ShowLobbyMenu(), PopupQueue.PopupFlag.HideOnTopmostMenuOpen, "ContentsNotice");
	}

	public void ReserveOpenClassBuffPopup()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenClassBuffPopup();
	}

	public void ReserveOpenSpecialMissionPopup()
	{
		List<MissionCategoryData> popupReservedSpecialMissionList = SceneAutoBehaviour<MissionManagement>.instance.GetPopupReservedSpecialMissionList();
		if (__Extension_Method_ICollection__.COUNT(popupReservedSpecialMissionList) == 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(popupReservedSpecialMissionList); i++)
		{
			MissionCategoryData missionData = popupReservedSpecialMissionList[i];
			if (missionData != null)
			{
				AddAction(delegate
				{
					SceneAutoBehaviour<MissionManagement>.instance.OpenSpecialMissionPopup(missionData);
				}, null, PopupQueue.PopupFlag.DefaultPopupFlag);
			}
		}
	}

	public void AddAction(Action openAction, Func<bool> isOpenFunc = null, PopupQueue.PopupFlag flag = PopupQueue.PopupFlag.None, string groupName = "")
	{
		_popupQueue.AddAction(openAction, isOpenFunc, flag, groupName);
	}

	public void RemoveAllClearedPopup(string groupName)
	{
		_popupQueue.RemoveAll(groupName);
	}
}
