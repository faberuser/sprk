using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NAchievement;
using NGame2.NContentsInfo;
using NGame2.NLobby;
using NGame2.NLogin;
using NGame2.NQuest;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.Mission;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.CompleteSubQuest;
using NShared.NLocalization;
using NShared.RewardAchievement;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.Mission;

public class MissionManagement : BaseManagement<MissionManagement>
{
	private MissionWindowManager _missionWindowManager;

	private MissionWebManager _missionWebManager;

	private Action _requestSubQuestReward;

	private bool RewardSkip;

	public MissionWindowManager MissionWindowManager
	{
		get
		{
			if (_missionWindowManager == null)
			{
				_missionWindowManager = new MissionWindowManager();
				_missionWindowManager.Init(this);
			}
			return _missionWindowManager;
		}
	}

	public MissionWebManager QuestIntegrationWebManager
	{
		get
		{
			if (_missionWebManager == null)
			{
				_missionWebManager = new MissionWebManager();
				_missionWebManager.Init(this);
			}
			return _missionWebManager;
		}
	}

	public string LocalizeHeroNotOwned => LocalizationManager.GetStringDirect("SUBSTORY_CHAPTER_NOT_CLEAR_DESC");

	protected override void Awake()
	{
		base.Awake();
		InitPrefab();
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		MissionMilestonePopup.noticeRewardSkip = (MissionMilestonePopup.NoticeRewardSkip)Delegate.Combine(MissionMilestonePopup.noticeRewardSkip, new MissionMilestonePopup.NoticeRewardSkip(NoticeRewardSkip));
	}

	private void OnDisable()
	{
		MissionMilestonePopup.noticeRewardSkip = (MissionMilestonePopup.NoticeRewardSkip)Delegate.Remove(MissionMilestonePopup.noticeRewardSkip, new MissionMilestonePopup.NoticeRewardSkip(NoticeRewardSkip));
	}

	private void NoticeRewardSkip()
	{
		RewardSkip = true;
	}

	private void InitPrefab()
	{
		AddInstance<MissionMain>("Resources/Prefab2/Window/QuestIntegration/MissionMain");
		AddInstance<MissionNodeWindow>("Resources/Prefab2/Window/QuestIntegration/MissionNodeWindow");
		AddInstance<MissionNode2Window>("Resources/Prefab2/Window/QuestIntegration/MissionNode2Window");
		AddInstance<MissionNodeWindowV3>("Resources/Prefab2/Window/QuestIntegration/MissionNodeWindowV3");
		AddInstance<MissionNodeWindowTopView2>("Resources/Prefab2/Window/QuestIntegration/MissionNodeWindowTopView2");
	}

	public void Move(string moveTaskType, string[] moveTaskValue)
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (missionMain != null)
		{
			missionMain.Close();
		}
		SceneAutoBehaviour<ActivityManager>.instance.Run(moveTaskType, moveTaskValue);
	}

	public void SetNewExtendMenu(int mainIndex, int subIndex, bool isNew)
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (!(missionMain == null))
		{
			_ = missionMain.CategoryMenuPanel == null;
		}
	}

	public LocalAccountString GetSpecialMissionPopupOpenLimitedTimeStorage(int mainCategoryIndex, int subCategoryIndex)
	{
		return new LocalAccountString("", "SpecialMissionPopupOpenLimitedTime_" + mainCategoryIndex + "_" + subCategoryIndex);
	}

	public LocalAccountString GetSpecialMissionPopupOpenableTimeStorage(int mainCategoryIndex, int subCategoryIndex)
	{
		return new LocalAccountString("", "SpecialMissionPopupOpenableTime_" + mainCategoryIndex + "_" + subCategoryIndex);
	}

	public void ResetSpecialMissionOpenLimitTime(int mainCategoryIndex, int subCategoryIndex)
	{
		GetSpecialMissionPopupOpenLimitedTimeStorage(mainCategoryIndex, subCategoryIndex).Value = string.Empty;
	}

	public void ResetSpecialMissionOpenableTime(int mainCategoryIndex, int subCategoryIndex)
	{
		GetSpecialMissionPopupOpenableTimeStorage(mainCategoryIndex, subCategoryIndex).Value = string.Empty;
	}

	public void SetSpecialMissionPopupNextOpenTime(bool isNextMidnight, int mainCategoryIndex, int subCategoryIndex)
	{
		DateTime serverLocalTime = GetServerLocalTime();
		LocalAccountString specialMissionPopupOpenLimitedTimeStorage = GetSpecialMissionPopupOpenLimitedTimeStorage(mainCategoryIndex, subCategoryIndex);
		string value = specialMissionPopupOpenLimitedTimeStorage.Value;
		if (String_Extension_Method.IsNullOrEmpty(value) || !Vespa_String_Extension_Method2.TryParseNetworkTime2(value, out var o_dateTime))
		{
			o_dateTime = serverLocalTime.AddHours(72.0);
			specialMissionPopupOpenLimitedTimeStorage.Value = Vespa_DateTime_Extension_Method.ToNetworkString2(o_dateTime);
		}
		if (isNextMidnight)
		{
			LocalAccountString specialMissionPopupOpenableTimeStorage = GetSpecialMissionPopupOpenableTimeStorage(mainCategoryIndex, subCategoryIndex);
			DateTime dateTime = serverLocalTime.AddDays(1.0);
			DateTime i_dateTime = NVespa.Math.CreateDateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			specialMissionPopupOpenableTimeStorage.Value = Vespa_DateTime_Extension_Method.ToNetworkString2(i_dateTime);
		}
		else
		{
			ResetSpecialMissionOpenableTime(mainCategoryIndex, subCategoryIndex);
		}
	}

	public void SetEventOrBoosterExtendMenu(int mainIndex, bool isEvent, bool isBooster)
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (!(missionMain == null))
		{
			_ = missionMain.CategoryMenuPanel == null;
		}
	}

	public void RequestAchievementReward(List<int> indexList, Action responseAction, float openDelayTime = 0f)
	{
		SceneAutoBehaviour<AchievementManagement>.instance.RequestRewardAchievementTask(indexList, delegate(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response, bool heroAchievement, Action afterReward)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(indexList); i++)
			{
				SingletonAuto<AchievementNotiChecker>.instance.RewardLimitedEventMission(indexList[i]);
			}
			ResponseSuccessQuestReward(request, response, heroAchievement, afterReward, openDelayTime);
			if (responseAction != null)
			{
				Vespa_System_Action_Extension_Method.fire(responseAction);
			}
		}).Commit();
	}

	private void ResponseSuccessQuestReward(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response, bool isHeroAchievement, Action afterReward, float openDelayTime = 0f)
	{
		BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Quest.COMPLETE_QUEST_MSG)).SetExpResult(response.expResult).SetItemResultInfos(response.itemResults)
			.SetEquipResultInfos(response.equipItemInfos)
			.SetFriendshipPointResults(response.FriendshipPointResults)
			.AddCurrencyResultInfos3(response.CurrencyResults);
		if (response.staminaResults != null)
		{
			popupOpenData.SetStaminaResultInfos(response.staminaResults);
		}
		if (response.ItemTimeDurationInfo != null)
		{
			PopupUseBoosterMsg(response.ItemTimeDurationInfo);
		}
		VespaCoroutine.StartGlobal(ResponseQuestRewardOpenPopup(popupOpenData, isGlobal: true, afterReward, openDelayTime));
	}

	private IEnumerator ResponseQuestRewardOpenPopup(BaseRewardPopup.PopupOpenData popupOpen, bool isGlobal, Action onClose, float OpenDelayTime)
	{
		if (OpenDelayTime > 0f)
		{
			float DelayTime = 0f;
			while (DelayTime < OpenDelayTime && !RewardSkip)
			{
				DelayTime += Time.deltaTime;
				yield return null;
			}
			if (RewardSkip)
			{
				yield return new WaitForSeconds(1f);
			}
		}
		RewardSkip = false;
		BaseRewardPopup.OpenPopup(popupOpen, isGlobal: true, onClose);
	}

	private void PopupUseBoosterMsg(PlayerItemTimeDurationInfo info)
	{
		if (info != null)
		{
			ItemData<BoosterItemData> data = GameRoot.Instance.ItemDataContainer.GetData<BoosterItemData>(info.ItemIndex);
			if (data != null)
			{
				TimeSpan remainTime = info.GetRemainTime(data);
				CenterMessage.Open(LocalizationManager.GetString(Item.USE_BOOSTER_MSG, ItemNameBuilder.GetItemName(data), (int)remainTime.TotalDays, remainTime.Hours, remainTime.Minutes));
			}
		}
	}

	public void RequestCompleteSubQuest(List<int> list, Action callbackAction = null, float openDelayTime = 0f)
	{
		if (QuestIntegrationWebManager != null)
		{
			_requestSubQuestReward = callbackAction;
			QuestIntegrationWebManager.RequestCompleteSubQuest(list, ResponseCompleteSubQuest, openDelayTime);
		}
	}

	private void ResponseCompleteSubQuest(NShared.CompleteSubQuest.Response response, float openDelayTime = 0f)
	{
		if (response == null)
		{
			return;
		}
		OpenHeroGrowthReward(response.RewardResults, delegate(bool isNoDelay)
		{
			if (isNoDelay)
			{
				OpenRewardPopup(response.RewardResults);
			}
			else
			{
				OpenRewardPopup(response.RewardResults, openDelayTime);
			}
			Vespa_System_Action_Extension_Method.fire(_requestSubQuestReward);
			MissionMain missionMain = GetInstance<MissionMain>();
			if (!(missionMain == null) && !(missionMain.MissionListPanel == null))
			{
				RefreshQuestList();
			}
		});
	}

	private void OpenHeroGrowthReward(RewardResultInfo[] infos, Action<bool> onEnd)
	{
		List<HeroInfo> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			RewardResultInfo rewardResultInfo = infos[i];
			if (rewardResultInfo != null && __Extension_Method_ICollection__.COUNT(rewardResultInfo.HeroInfos) != 0)
			{
				if (list == null)
				{
					list = new List<HeroInfo>();
				}
				list.AddRange(rewardResultInfo.HeroInfos);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onEnd, i_argA: false);
			return;
		}
		GlobalAutoBehaviour<AccountManager>.instance.ApplyHeroGrowthReward(list, delegate(bool isNoDelay)
		{
			OpenTeamExpReward(infos);
			Vespa_System_Action_Extension_Method.fire(onEnd, isNoDelay);
		});
	}

	private void OpenTeamExpReward(RewardResultInfo[] infos)
	{
		List<ExpResultInfo> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			RewardResultInfo rewardResultInfo = infos[i];
			if (rewardResultInfo != null && __Extension_Method_ICollection__.COUNT(rewardResultInfo.ExpResultInfos) != 0)
			{
				if (list == null)
				{
					list = new List<ExpResultInfo>();
				}
				list.AddRange(rewardResultInfo.ExpResultInfos);
			}
		}
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(list);
	}

	private void OpenRewardPopup(RewardResultInfo[] infos, float openDelayTime = 0f)
	{
		if (infos == null)
		{
			return;
		}
		BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(ScenarioQuest.REWARD_TITLE));
		List<string> boosterApplyMessageList = null;
		Array.ForEach(infos, delegate(RewardResultInfo e)
		{
			popupOpen.AddItemResultInfos(e.ItemResults).AddEquipResultInfos(e.EquipItemResults).AddCurrencyResultInfos3(e.CurrencyResults)
				.AddStaminaResultInfos(e.StaminaResultInfos)
				.AddPetInfos(e.PetInfos);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(e.ItemTimeDurationInfos); i++)
			{
				PlayerItemTimeDurationInfo playerItemTimeDurationInfo = e.ItemTimeDurationInfos[i];
				if (playerItemTimeDurationInfo != null)
				{
					string boosterApplyString = UIHelper.GetBoosterApplyString(playerItemTimeDurationInfo);
					if (!String_Extension_Method.IsNullOrEmpty(boosterApplyString))
					{
						if (boosterApplyMessageList == null)
						{
							boosterApplyMessageList = new List<string>();
						}
						boosterApplyMessageList.Add(boosterApplyString);
					}
				}
			}
		});
		if (openDelayTime > 0f)
		{
			VespaCoroutine.StartGlobal(ResponseQuestRewardOpenPopup(popupOpen, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI(), delegate
			{
				OpenBoosterRewardInfoMessage(boosterApplyMessageList);
			}, openDelayTime));
			return;
		}
		RewardSkip = false;
		BaseRewardPopup.OpenPopup(popupOpen, isGlobal: true, delegate
		{
			OpenBoosterRewardInfoMessage(boosterApplyMessageList);
		});
	}

	private void OpenBoosterRewardInfoMessage(List<string> boosterApplyMessageList)
	{
		if (__Extension_Method_ICollection__.COUNT(boosterApplyMessageList) != 0)
		{
			MultiCenterMessages multiCenterMessages = MultiCenterMessages.Open();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(boosterApplyMessageList); i++)
			{
				string message = boosterApplyMessageList[i];
				multiCenterMessages.AddMessage(message);
			}
			multiCenterMessages.RepositionMessage();
		}
	}

	public DateTime GetServerLocalTime()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
	}

	public List<MissionCategoryData> GetPopupReservedSpecialMissionList()
	{
		List<MissionCategoryData> specialMissionList = GameRoot.Instance.MissionCategoryDataContainer.GetSpecialMissionList();
		List<MissionCategoryData> list = new List<MissionCategoryData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(specialMissionList); i++)
		{
			MissionCategoryData missionCategoryData = specialMissionList[i];
			if (CanReserveSpecialMissionPopup(missionCategoryData))
			{
				list.Add(missionCategoryData);
			}
		}
		return list;
	}

	public bool CanReserveSpecialMissionPopup(MissionCategoryData missionData)
	{
		if (missionData == null || !missionData.IsReservePopup || !IsOpenSubCategory(missionData))
		{
			return false;
		}
		if (IsSpecialMissionPopupLimitedTimeOver(missionData.MainCategoryIndex, missionData.SubCategoryIndex))
		{
			return false;
		}
		if (!IsSpecialMissionPopupOpenableTime(missionData.MainCategoryIndex, missionData.SubCategoryIndex))
		{
			return false;
		}
		return true;
	}

	public void OpenSpecialMissionPopup(MissionCategoryData data)
	{
		if (data != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.QuestManager.SetSpecialMissionPopupOpen(data.MainCategoryIndex, data.SubCategoryIndex, open: true);
			SpecialMissionNoticePopup.OpenPopup(data);
		}
	}

	private bool IsSpecialMissionPopupLimitedTimeOver(int mainCategoryIndex, int subCategoryIndex)
	{
		string value = GetSpecialMissionPopupOpenLimitedTimeStorage(mainCategoryIndex, subCategoryIndex).Value;
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(value, out var o_dateTime))
		{
			return false;
		}
		DateTime serverLocalTime = GetServerLocalTime();
		if (o_dateTime.CompareTo(serverLocalTime) < 0)
		{
			return true;
		}
		return false;
	}

	private bool IsSpecialMissionPopupOpenableTime(int mainCategoryIndex, int subCategoryIndex)
	{
		string value = GetSpecialMissionPopupOpenableTimeStorage(mainCategoryIndex, subCategoryIndex).Value;
		bool specialMissionPopupOpen = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSpecialMissionPopupOpen(mainCategoryIndex, subCategoryIndex);
		if (String_Extension_Method.IsNullOrEmpty(value))
		{
			return !specialMissionPopupOpen;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(value, out var o_dateTime))
		{
			return !specialMissionPopupOpen;
		}
		if (GetServerLocalTime().CompareTo(o_dateTime) < 0)
		{
			return false;
		}
		return true;
	}

	public void OpenCenterMessageMainQuestConditionFail()
	{
		if (!AccountManager.IsLogined())
		{
			return;
		}
		MainQuestInfo mainQuestInfo = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestInfo;
		if (mainQuestInfo != null)
		{
			MainQuestData dataByStep = GameRoot.Instance.MainQuestDataContainer.GetDataByStep(mainQuestInfo.Step);
			if (dataByStep != null)
			{
				string localizeQuestTitle = SingletonUpdater<QuestManager>.instance.GetLocalizeQuestTitle(dataByStep);
				CenterMessage.Open(LocalizeMainMissionConditionFail(localizeQuestTitle));
			}
		}
	}

	public void OnRewardTimeEnded()
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (!(missionMain == null) && !(missionMain.MissionListPanel == null) && !(missionMain.MissionListPanel.QuestListPanel == null))
		{
			missionMain.MissionListPanel.QuestListPanel.ForceUpdateDailyEventAchievement();
		}
	}

	public void RefreshQuestList()
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (!(missionMain == null) && !(missionMain.MissionListPanel == null) && !(missionMain.MissionListPanel.QuestListPanel == null))
		{
			missionMain.MissionListPanel.QuestListPanel.RefreshList();
			missionMain.Refresh();
		}
	}

	public void ResponseReward()
	{
		MissionMain missionMain = GetInstance<MissionMain>();
		if (!(missionMain == null))
		{
			missionMain.ResponseReward();
			MissionMilestonePopup.ReceiveReward();
		}
	}

	private string LocalizeMainMissionConditionFail(string message)
	{
		return LocalizationManager.GetStringDirect("MAIN_MISSION_CONDITION_FAIL", message);
	}

	public string LocalizeSubQuestGroup(int group)
	{
		return LocalizationManager.GetStringDirect("QUEST_GROUP_TITLE_" + group);
	}

	public static bool IsOpenMainCategory(int mainCategoryIndex)
	{
		for (MissionCategoryData missionCategoryData = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(mainCategoryIndex); missionCategoryData != null; missionCategoryData = missionCategoryData.GetNextData())
		{
			if (IsOpenSubCategory(missionCategoryData))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsOpenSubCategory(MissionCategoryData missionCategoryData)
	{
		return IsOpenSubCategory(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex);
	}

	public static bool IsOpenSubCategory(int mainCategoryIndex, int subCategoryIndex)
	{
		MissionCategoryInfo[] missionCategoryInfo = LoginData.MissionCategoryInfo;
		if (missionCategoryInfo == null)
		{
			return SprkAdventureUnlocks.MissionCategory(serverOpen: false, mainCategoryIndex, subCategoryIndex);
		}
		MissionCategoryInfo[] array = missionCategoryInfo;
		foreach (MissionCategoryInfo missionCategoryInfo2 in array)
		{
			if (missionCategoryInfo2.MainCategoryIndex == mainCategoryIndex && missionCategoryInfo2.SubCategoryIndex == subCategoryIndex)
			{
				return SprkAdventureUnlocks.MissionCategory(serverOpen: true, mainCategoryIndex, subCategoryIndex);
			}
		}
		return SprkAdventureUnlocks.MissionCategory(serverOpen: false, mainCategoryIndex, subCategoryIndex);
	}

	public static bool IsMissionMainRewardCheck()
	{
		IList<MissionCategoryData> allData = GameRoot.Instance.MissionCategoryDataContainer.GetAllData();
		List<MissionCategoryData> list = new List<MissionCategoryData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			MissionCategoryData missionCategoryData = allData[i];
			if (missionCategoryData != null && !SingletonAuto<MissionStatus>.instance.IsMissionTimeout(missionCategoryData))
			{
				list.Add(missionCategoryData);
			}
		}
		return IsRewardCheck(list);
	}

	public static bool IsMissionEventCategoryRewardCheck()
	{
		return IsRewardCheck(GameRoot.Instance.MissionCategoryDataContainer.GetWebEventMissionList());
	}

	public static bool IsMissionSpecialCategoryRewardCheck()
	{
		List<MissionCategoryData> list = new List<MissionCategoryData>();
		List<MissionCategoryData> specialMissionList = GameRoot.Instance.MissionCategoryDataContainer.GetSpecialMissionList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(specialMissionList); i++)
		{
			MissionCategoryData missionCategoryData = specialMissionList[i];
			if (!SingletonAuto<MissionStatus>.instance.IsMissionTimeout(missionCategoryData))
			{
				list.Add(missionCategoryData);
			}
		}
		return IsRewardCheck(list);
	}

	public static bool IsMissionCategoryRewardCheck(int mainCategoryIndex)
	{
		return IsRewardCheck(GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(mainCategoryIndex));
	}

	public static bool IsMissionSubCategoryRewardCheck(int mainCategoryIndex, int subCategoryIndex)
	{
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(mainCategoryIndex, subCategoryIndex);
		return IsRewardCheck(new List<MissionCategoryData> { data });
	}

	private static bool IsRewardCheckQuestTypeAndQuestIndex(string questType, int questIndex)
	{
		if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
		{
			return false;
		}
		bool result = false;
		if (questType.CompareTo("Achievement") == 0)
		{
			result = !AchievementChecker.Completed(questIndex) && AchievementChecker.Cleared(questIndex);
		}
		else if (questType.CompareTo("SubQuest") == 0)
		{
			result = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetRewardableSubQuest(questIndex);
		}
		return result;
	}

	private static bool IsRewardCheck(List<MissionCategoryData> categoryList)
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(categoryList); i++)
		{
			MissionCategoryData missionCategoryData = categoryList[i];
			if (missionCategoryData.IsHeroMission())
			{
				if (!flag)
				{
					flag = true;
					if (IsHeroAchievementRewardCheck())
					{
						return true;
					}
				}
			}
			else if (missionCategoryData.IsWebEvent())
			{
				if (!flag2)
				{
					flag2 = true;
					if (IsWebEventRewardCheck())
					{
						return true;
					}
				}
			}
			else
			{
				string rewardQuestType = missionCategoryData.RewardQuestType;
				int rewardQuestIndex = missionCategoryData.RewardQuestIndex;
				if (IsRewardCheckQuestTypeAndQuestIndex(rewardQuestType, rewardQuestIndex))
				{
					return true;
				}
			}
			List<MissionVisualData> missionVisualDataList = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex);
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(missionVisualDataList); j++)
			{
				MissionVisualData missionVisualData = missionVisualDataList[j];
				string questType = missionVisualData.QuestType;
				int questIndex = missionVisualData.QuestIndex;
				if (IsRewardCheckQuestTypeAndQuestIndex(questType, questIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsWebEventRewardCheck()
	{
		WebEventInfo webEvent = LoginData.WebEvent;
		if (webEvent != null)
		{
			WebContentInfo[] webContents = webEvent.WebContents;
			for (int i = 0; i < webContents.Length; i++)
			{
				if (webContents[i].HasAReward)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsHeroAchievementRewardCheck()
	{
		HashSet<int> allHeroAchievementIndices = GameRoot.Instance.AchievementDataContainer.GetAllHeroAchievementIndices();
		if (allHeroAchievementIndices == null)
		{
			return false;
		}
		foreach (int item in allHeroAchievementIndices)
		{
			if (!AchievementChecker.Completed(item) && AchievementChecker.Cleared(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNewEventCategory()
	{
		if (LoginData.WebEvent == null)
		{
			return false;
		}
		if (LoginData.WebEvent.WebContents == null)
		{
			return false;
		}
		WebContentInfo[] webContents = LoginData.WebEvent.WebContents;
		foreach (WebContentInfo webContentInfo in webContents)
		{
			if (!SingletonAuto<PlayerPrefsOpenHistoryData>.instance.IsOpened_WebContent(webContentInfo.Index, webContentInfo.BeginTime))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNewMainCategory(int mainCategoryIndex)
	{
		List<MissionCategoryData> missionCategoryList = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(mainCategoryIndex);
		if (missionCategoryList == null)
		{
			return false;
		}
		foreach (MissionCategoryData item in missionCategoryList)
		{
			if (IsOpenSubCategory(item) && IsNewSubCategory(item.MainCategoryIndex, item.SubCategoryIndex))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNewSubCategory(int mainCategoryIndex, int subCategoryIndex)
	{
		return !SingletonAuto<PlayerPrefsOpenHistoryData>.instance.IsOpened_NewMark(mainCategoryIndex, subCategoryIndex);
	}

	public static bool IsNewMissionMain()
	{
		IList<MissionCategoryData> allData = GameRoot.Instance.MissionCategoryDataContainer.GetAllData();
		if (allData == null)
		{
			return false;
		}
		foreach (MissionCategoryData item in allData)
		{
			if (IsOpenSubCategory(item) && IsNewSubCategory(item.MainCategoryIndex, item.SubCategoryIndex))
			{
				return true;
			}
		}
		return false;
	}

	public static void CheckNewMark(int mainCategoryIndex, int subCategoryIndex)
	{
		SingletonAuto<PlayerPrefsOpenHistoryData>.instance.SetOpen_NewMark(mainCategoryIndex, subCategoryIndex);
	}

	public static void CheckNewMark(int mainCategoryIndex)
	{
		List<MissionCategoryData> missionCategoryList = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(mainCategoryIndex);
		if (missionCategoryList == null)
		{
			return;
		}
		foreach (MissionCategoryData item in missionCategoryList)
		{
			if (IsOpenSubCategory(item))
			{
				CheckNewMark(mainCategoryIndex, item.SubCategoryIndex);
			}
		}
	}

	public override void CloseAllActiveWindow(bool immediately = false, IEnumerable<IWindow> exceptions = null)
	{
		base.CloseAllActiveWindow(immediately, exceptions);
		ChangeTopMostMenuLayer(GameUIRoot.ELayerType.UI);
	}

	public void ChangeTopMostMenuLayer(GameUIRoot.ELayerType layerType)
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<LobbyTopmostMenu>())
		{
			LobbyTopmostMenu window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<LobbyTopmostMenu>();
			if (window != null)
			{
				LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(layerType);
				window.ChangeLayerRecursive(layerInfo);
			}
		}
	}
}
