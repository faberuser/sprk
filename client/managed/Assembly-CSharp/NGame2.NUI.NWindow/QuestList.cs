using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NLogin;
using NGame2.NScene;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.Mission;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class QuestList : BaseWindow, MissionComponent.IListner, MissionCategoryPanel.IListener, WindowAnimation.IAlphaAnimator, MissionCategoryComponent.IListner
{
	public enum EListType
	{
		List1,
		List2
	}

	[Header("[QuestList]")]
	[Header("--<List1>")]
	public Transform List1Transform;

	public UIScrollGrid List1Grid;

	public UIButton GetAllRewardButton;

	public Transform GetAllDisableTransform;

	[Header("--<List2>")]
	public Transform List2Transform;

	public UIScrollGrid List2Grid;

	public UILabel RemainingTimer;

	public List<MissionCategoryComponent> SubCategoryButtons;

	[Header("----(QuestRewardUI)")]
	public QuestRewardUI QuestRewardUI;

	private Dictionary<Tuple<EQuestType, int>, MissionComponent> _DicMissionComponents = new Dictionary<Tuple<EQuestType, int>, MissionComponent>();

	private EListType ListGridType = EListType.List2;

	private UIScrollGrid ListGrid;

	private List<Tuple<EQuestType, int>> _ListClearedNodeIndex = new List<Tuple<EQuestType, int>>();

	private List<Tuple<EQuestType, int>> _ListNotClearedNodeIndex = new List<Tuple<EQuestType, int>>();

	private List<Tuple<EQuestType, int>> _ListCompletedNodeIndex = new List<Tuple<EQuestType, int>>();

	private List<Tuple<EQuestType, int>> _ListNotAvailableNodeIndex = new List<Tuple<EQuestType, int>>();

	private int _CheckDailyEventQuestHour = -1;

	private DateTime _UpdateTime = DateTime.MinValue;

	private float _lastCheckedQuestRemainTime;

	private float _lastCheckedHelpTouched;

	private float _lastCheckedGrid;

	private QuestListCompare NodeIndexCompare = new QuestListCompare();

	private Dictionary<int, string> _DicInGameEventItemCode = new Dictionary<int, string>();

	private int SelectedMainCategory { get; set; }

	private int SelectedSubCategory { get; set; }

	private static MissionWindowManager GetMissionWindowManager()
	{
		if (!SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			return null;
		}
		return SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager;
	}

	private void OnEnable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Combine(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
		MissionMilestonePopup.noticeCloseMissionPopup = (MissionMilestonePopup.NoticeCloseMissionPopup)Delegate.Combine(MissionMilestonePopup.noticeCloseMissionPopup, new MissionMilestonePopup.NoticeCloseMissionPopup(NoticeCloseMissionPopup));
	}

	private void OnDisable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Remove(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
		MissionMilestonePopup.noticeCloseMissionPopup = (MissionMilestonePopup.NoticeCloseMissionPopup)Delegate.Remove(MissionMilestonePopup.noticeCloseMissionPopup, new MissionMilestonePopup.NoticeCloseMissionPopup(NoticeCloseMissionPopup));
	}

	public override void Open()
	{
		Open(delegate
		{
			Init();
		});
	}

	public void Init()
	{
		RunOpenCloseAnimation(reversse: false);
		DefaultCategory();
	}

	public override void ReOpen()
	{
		base.ReOpen();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
		DefaultCategory();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (NeedUpdateDailyEventAchievement())
		{
			UpdateDailyEventAchievement();
		}
		if (Time.time - _lastCheckedQuestRemainTime >= 1f)
		{
			UpdateQuestRemainTime();
			_lastCheckedQuestRemainTime = Time.time;
		}
		if (Time.time - _lastCheckedGrid >= 0.3f)
		{
			UpdateGridParticleEffectRemove();
			_lastCheckedGrid = Time.time;
		}
	}

	private void UpdateGridParticleEffectRemove()
	{
		if (ListGrid == null)
		{
			return;
		}
		MissionComponent[] componentsInChildren = ListGrid.GetComponentsInChildren<MissionComponent>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		MissionComponent[] array = componentsInChildren;
		foreach (MissionComponent obj in array)
		{
			bool RewardEffect = true;
			Vector3 localPosition = obj.transform.localPosition;
			localPosition.y += 20f;
			UIHelper.CheckOutOfGridSightItem(ListGrid.Grid, localPosition, delegate
			{
				RewardEffect = false;
			}, null);
			obj.EnableRewardParticle(RewardEffect);
		}
	}

	private void NoticeOpenMissionPopup()
	{
		RunOpenCloseAnimation(reversse: true);
	}

	private void NoticeCloseMissionPopup()
	{
		RunOpenCloseAnimation(reversse: false);
	}

	private void UpdateQuestRemainTime()
	{
		SetupList2RemainTime();
	}

	public void RefreshList()
	{
		Clear();
		SetupQuestList();
		SetupList2RemainTime();
		if (QuestRewardUI != null)
		{
			QuestRewardUI.Refresh(__Extension_Method_ICollection__.COUNT(_ListClearedNodeIndex) > 0);
		}
		if (SubCategoryButtons != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SubCategoryButtons); i++)
			{
				SubCategoryButtons[i].Refresh();
			}
		}
		bool flag = false;
		if (_ListClearedNodeIndex != null && __Extension_Method_ICollection__.COUNT(_ListClearedNodeIndex) > 0)
		{
			int num = 0;
			foreach (Tuple<EQuestType, int> item in _ListClearedNodeIndex)
			{
				if (_DicMissionComponents.TryGetValue(item, out var value))
				{
					MissionComponentData elementData = value.ElementData;
					if (elementData != null && elementData.GetData<AchievementData>() != null && elementData.GetData<AchievementData>().RewardType == AchievementRewardType.Booster)
					{
						num++;
					}
				}
			}
			flag = __Extension_Method_ICollection__.COUNT(_ListClearedNodeIndex) - num > 0;
		}
		UIHelper.EnableGameObject(GetAllRewardButton, flag);
		UIHelper.EnableGameObject(GetAllDisableTransform, !flag);
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_DicMissionComponents);
		QuestRewardUI.Clear();
	}

	private void RunOpenCloseAnimation(bool reversse)
	{
		WindowAnimation component = GetComponent<WindowAnimation>();
		if (component == null)
		{
			VespaDebug.LogError("QuestList Null WindowAnimation");
		}
		else
		{
			component.PlayAnimation(reversse);
		}
	}

	private void SetupQuestList()
	{
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(SelectedMainCategory, SelectedSubCategory);
		if (data != null)
		{
			if (ListGrid != null)
			{
				UIHelper.GridOriginPositioning(ListGrid.Grid);
			}
			CreateMissionListGrid(data);
		}
	}

	public void SetupWebEvents(ref List<Tuple<EQuestType, int>> listNodeIndex)
	{
		WebEventInfo webEvent = LoginData.WebEvent;
		if (webEvent == null || webEvent.WebContents == null || webEvent.WebContents.Length == 0)
		{
			return;
		}
		for (int i = 0; i < webEvent.WebContents.Length; i++)
		{
			WebContentInfo webContentInfo = webEvent.WebContents[i];
			if (webContentInfo != null && webContentInfo.DisplayEvent != 0 && (String_Extension_Method.IsNullOrEmpty(webContentInfo.DisplayOnTime) || (Vespa_String_Extension_Method2.TryParseNetworkTime2(webContentInfo.DisplayOnTime, out var o_dateTime) && !((o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalMilliseconds > 0.0))) && (String_Extension_Method.IsNullOrEmpty(webContentInfo.DisplayOffTime) || (Vespa_String_Extension_Method2.TryParseNetworkTime2(webContentInfo.DisplayOffTime, out var o_dateTime2) && !((o_dateTime2 - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalMilliseconds < 0.0))))
			{
				listNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.WebContent, webContentInfo.Index));
			}
		}
	}

	public void SetupAttendanceInfo(ref List<Tuple<EQuestType, int>> listNodeIndex)
	{
		foreach (AttendanceKeywordGroup item in AttendanceFacade.GetGroupList_InProgress())
		{
			listNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Attendance, item.IndexMain));
		}
	}

	public void SetupInGameEvents(ref List<Tuple<EQuestType, int>> listNodeIndex)
	{
		_DicInGameEventItemCode.Clear();
		List<BoosterInfo> allEffectiveBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveBooster) <= 0)
		{
			return;
		}
		int num = 0;
		foreach (BoosterInfo item in allEffectiveBooster)
		{
			if (String_Extension_Method.IsNullOrEmpty(item.ItemCode))
			{
				continue;
			}
			if (item.BuffPosition == DropBonusEvnetBuffPosition.None)
			{
				if (NGame2.NUI.Common.IsShowBoosterInfoList(item.BonusType))
				{
					continue;
				}
			}
			else if (item.BuffPosition != DropBonusEvnetBuffPosition.Left)
			{
				continue;
			}
			string value = $"{item.ItemCode}_{item.Name}";
			_DicInGameEventItemCode.Add(num, value);
			listNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.InGameEvent, num));
			num++;
		}
	}

	private void CreateMissionListGrid(MissionCategoryData selectedData)
	{
		_ListClearedNodeIndex.Clear();
		_ListNotClearedNodeIndex.Clear();
		_ListCompletedNodeIndex.Clear();
		_ListNotAvailableNodeIndex.Clear();
		foreach (MissionVisualData missionVisualData in GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataList(SelectedMainCategory, SelectedSubCategory))
		{
			string questType = missionVisualData.QuestType;
			int questIndex = missionVisualData.QuestIndex;
			if (String_Extension_Method.IsNullOrEmpty(questType) || questIndex == 0)
			{
				continue;
			}
			if (questType.CompareTo("Achievement") == 0)
			{
				AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(questIndex, 1);
				if (data != null && data.ReqType == AchievementReqType.LoginOrConnectedTime)
				{
					SetupLoginOrConnectedData();
					continue;
				}
				if (AchievementChecker.Completed(questIndex))
				{
					_ListCompletedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, questIndex));
					continue;
				}
				if (AchievementChecker.Cleared(questIndex))
				{
					_ListClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, questIndex));
					continue;
				}
				bool num = SingletonAuto<AchievementChecker>.instance.Available(questIndex);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(questIndex) == null;
				if (!num || flag)
				{
					_ListNotAvailableNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, questIndex));
				}
				else
				{
					_ListNotClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, questIndex));
				}
			}
			else if (questType.CompareTo("SubQuest") == 0)
			{
				SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(questIndex);
				switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(subQuestData))
				{
				case QuesetStatusType.Complete:
					_ListCompletedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.SubQuest, questIndex));
					break;
				case QuesetStatusType.Reward:
					_ListClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.SubQuest, questIndex));
					break;
				case QuesetStatusType.Progress:
					_ListNotClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.SubQuest, questIndex));
					break;
				default:
					_ListNotAvailableNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.SubQuest, questIndex));
					break;
				}
			}
		}
		if (selectedData.IsWebEvent())
		{
			SetupWebEvents(ref _ListNotClearedNodeIndex);
			SetupAttendanceInfo(ref _ListNotClearedNodeIndex);
			SetupInGameEvents(ref _ListNotClearedNodeIndex);
		}
		_ListClearedNodeIndex.Sort(NodeIndexCompare);
		_ListNotClearedNodeIndex.Sort(NodeIndexCompare);
		_ListCompletedNodeIndex.Sort(NodeIndexCompare);
		_ListNotAvailableNodeIndex.Sort(NodeIndexCompare);
		_ListClearedNodeIndex.ForEach(delegate(Tuple<EQuestType, int> Key)
		{
			CreateMissionComponent(Key.Item1, Key.Item2);
		});
		_ListNotClearedNodeIndex.ForEach(delegate(Tuple<EQuestType, int> Key)
		{
			CreateMissionComponent(Key.Item1, Key.Item2);
		});
		_ListNotAvailableNodeIndex.ForEach(delegate(Tuple<EQuestType, int> Key)
		{
			CreateMissionComponent(Key.Item1, Key.Item2, Completed: false, Available: false);
		});
		_ListCompletedNodeIndex.ForEach(delegate(Tuple<EQuestType, int> Key)
		{
			CreateMissionComponent(Key.Item1, Key.Item2, Completed: true);
		});
		UIHelper.RepositionGrid(ListGrid);
		SetupSubCategoryReward();
	}

	private void SetupLoginOrConnectedData()
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		int loginOrConnectedAchievementIndex = GameRoot.Instance.AchievementDataContainer.GetLoginOrConnectedAchievementIndex(currentServerLocalTime.Hour);
		AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(loginOrConnectedAchievementIndex, 1);
		if (data == null)
		{
			return;
		}
		if (AchievementChecker.Completed(data.Index))
		{
			int hourOfDay = (int)data.ReqSub2Value;
			int loginOrConnectedAchievementIndex2 = GameRoot.Instance.AchievementDataContainer.GetLoginOrConnectedAchievementIndex(hourOfDay);
			if (loginOrConnectedAchievementIndex2 != 0 && !AchievementChecker.Cleared(loginOrConnectedAchievementIndex2))
			{
				_ListNotClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, loginOrConnectedAchievementIndex2));
			}
			if (loginOrConnectedAchievementIndex2 == 0)
			{
				_ListCompletedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, data.Index));
			}
		}
		else if (AchievementChecker.Cleared(data.Index))
		{
			_ListClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, data.Index));
		}
		else
		{
			_ListNotClearedNodeIndex.Add(new Tuple<EQuestType, int>(EQuestType.Achievement, data.Index));
		}
	}

	private void CreateMissionComponent(EQuestType questType, int Index, bool Completed = false, bool Available = true, MissionVisualData missionVisualData = null)
	{
		if (ListGrid == null && !SetQuestListUI(ListGridType))
		{
			VespaDebug.LogError("ListType Error. " + ListGridType);
			return;
		}
		MissionCategoryData missionCategoryData = null;
		int num = 0;
		if (missionVisualData != null)
		{
			missionCategoryData = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(missionVisualData.MainCategoryIndex);
			num = missionCategoryData?.MissionComponentValue ?? 0;
			if (SingletonAuto<MissionStatus>.instance.TryGetRemainTime(missionCategoryData, out var remainTimeSpan) && remainTimeSpan <= TimeSpan.Zero)
			{
				return;
			}
		}
		switch (questType)
		{
		case EQuestType.Achievement:
		{
			MissionComponent missionComponent3 = UIHelper.AddGridItem(ListGrid, MissionComponent.PrefabAuto);
			MissionComponentData missionComponentData3 = new MissionComponentData(Index, missionVisualData);
			if (missionVisualData != null)
			{
				missionComponent3.ExpansionQuickOpen(missionComponentData3, this, num);
			}
			else
			{
				missionComponent3.ExpansionOpen(missionComponentData3, this, Available);
			}
			Tuple<EQuestType, int> key3 = new Tuple<EQuestType, int>(questType, Index);
			_DicMissionComponents.Add(key3, missionComponent3);
			break;
		}
		case EQuestType.SubQuest:
		{
			SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(Index);
			if (subQuestData == null)
			{
				VespaDebug.LogError("QuestList SubQuestData is null. " + Index);
				break;
			}
			MissionComponent missionComponent4 = UIHelper.AddGridItem(ListGrid, MissionComponent.PrefabAuto);
			MissionComponentData missionComponentData4 = new MissionComponentData(subQuestData, missionVisualData);
			if (missionVisualData != null)
			{
				missionComponent4.ExpansionQuickOpen(missionComponentData4, this, num);
			}
			else
			{
				missionComponent4.ExpansionOpen(missionComponentData4, this, Available);
			}
			Tuple<EQuestType, int> key4 = new Tuple<EQuestType, int>(questType, Index);
			_DicMissionComponents.Add(key4, missionComponent4);
			break;
		}
		case EQuestType.WebContent:
		{
			WebEventInfo webEvent = LoginData.WebEvent;
			if (webEvent == null || webEvent.WebContents == null || webEvent.WebContents.Length == 0)
			{
				break;
			}
			WebContentInfo webContentInfo = null;
			for (int i = 0; i < webEvent.WebContents.Length; i++)
			{
				WebContentInfo webContentInfo2 = webEvent.WebContents[i];
				if (webContentInfo2.Index == Index)
				{
					webContentInfo = webContentInfo2;
					break;
				}
			}
			if (webContentInfo == null)
			{
				break;
			}
			Vespa_String_Extension_Method2.TryParseNetworkTime2(webContentInfo.EndTime, out var o_dateTime);
			if (Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0) > 0)
			{
				MissionComponent missionComponent5 = UIHelper.AddGridItem(ListGrid, MissionComponent.PrefabAuto);
				MissionComponentData missionComponentData5 = new MissionComponentData(webContentInfo, missionVisualData);
				if (missionVisualData != null)
				{
					missionComponent5.ExpansionQuickOpen(missionComponentData5, this, num);
				}
				else
				{
					missionComponent5.ExpansionOpen(missionComponentData5, this, Available);
				}
				Tuple<EQuestType, int> key5 = new Tuple<EQuestType, int>(questType, Index);
				_DicMissionComponents.Add(key5, missionComponent5);
			}
			break;
		}
		case EQuestType.Attendance:
		{
			AttendanceKeywordGroup attendanceKeywordGroup = AttendanceKeywordGroup.Helper.CreateByAnyAttendanceIndex(Index);
			if (attendanceKeywordGroup.IsValid_AttendanceData())
			{
				MissionComponent missionComponent2 = UIHelper.AddGridItem(ListGrid, MissionComponent.PrefabAuto);
				MissionComponentData missionComponentData2 = new MissionComponentData(attendanceKeywordGroup.AttendanceData, missionVisualData);
				if (missionVisualData != null)
				{
					missionComponent2.ExpansionQuickOpen(missionComponentData2, this, num);
				}
				else
				{
					missionComponent2.ExpansionOpen(missionComponentData2, this, Available);
				}
				Tuple<EQuestType, int> key2 = new Tuple<EQuestType, int>(questType, Index);
				_DicMissionComponents.Add(key2, missionComponent2);
			}
			break;
		}
		case EQuestType.InGameEvent:
		{
			List<BoosterInfo> allEffectiveBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster();
			if (!_DicInGameEventItemCode.ContainsKey(Index))
			{
				break;
			}
			BoosterInfo boosterInfo = null;
			foreach (BoosterInfo item in allEffectiveBooster)
			{
				string text = $"{item.ItemCode}_{item.Name}";
				if (_DicInGameEventItemCode[Index] == text)
				{
					boosterInfo = item;
					break;
				}
			}
			if (boosterInfo != null && !String_Extension_Method.IsNullOrEmpty(boosterInfo.ItemCode) && !String_Extension_Method.IsNullOrEmpty(boosterInfo.Name))
			{
				MissionComponent missionComponent = UIHelper.AddGridItem(ListGrid, MissionComponent.PrefabAuto);
				MissionComponentData missionComponentData = new MissionComponentData(new BoosterInfo(boosterInfo), missionVisualData);
				if (missionVisualData != null)
				{
					missionComponent.ExpansionQuickOpen(missionComponentData, this, num);
				}
				else
				{
					missionComponent.ExpansionOpen(missionComponentData, this, Available);
				}
				Tuple<EQuestType, int> key = new Tuple<EQuestType, int>(questType, Index);
				_DicMissionComponents.Add(key, missionComponent);
			}
			break;
		}
		}
	}

	public void DefaultCategory()
	{
		SelectMainCategory(2, isForce: true);
	}

	public void SelectMainCategory(int mainCategoryIndex, bool isForce = false)
	{
		switch (EnumTools.Parse(GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(mainCategoryIndex).UIPropName, EUIPropType.Null))
		{
		case EUIPropType.List1:
			SelectMainCategory(EListType.List1, mainCategoryIndex);
			break;
		case EUIPropType.List2:
			SelectMainCategory(EListType.List2, mainCategoryIndex, isForce);
			break;
		}
	}

	public void SelectMainCategory(EListType listType, int mainCategoryIndex, bool isForce = false)
	{
		if (SelectedMainCategory != mainCategoryIndex || isForce)
		{
			SelectedMainCategory = mainCategoryIndex;
			SelectedSubCategory = 1;
			ListGridType = listType;
			if (SetQuestListUI(ListGridType))
			{
				RefreshList();
			}
		}
	}

	public void SelectSubCategory(int subCategoryIndex)
	{
		if (GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(SelectedMainCategory) != null && SelectedSubCategory != subCategoryIndex)
		{
			SelectedSubCategory = subCategoryIndex;
			RefreshList();
		}
	}

	private bool SetQuestListUI(EListType listType)
	{
		if (ListGridType == EListType.List1)
		{
			ListGrid = List1Grid;
			UIHelper.EnableGameObject(List1Transform, enable: true);
			UIHelper.EnableGameObject(List2Transform, enable: false);
			UIHelper.EnableGameObject(QuestRewardUI.gameObject, enable: false);
		}
		else
		{
			if (ListGridType != EListType.List2)
			{
				ListGrid = null;
				UIHelper.EnableGameObject(List1Transform, enable: false);
				UIHelper.EnableGameObject(List2Transform, enable: false);
				UIHelper.EnableGameObject(QuestRewardUI.gameObject, enable: false);
				VespaDebug.LogError("QuestList ListType is Wrong. " + ListGridType);
				return false;
			}
			ListGrid = List2Grid;
			UIHelper.EnableGameObject(List1Transform, enable: false);
			UIHelper.EnableGameObject(List2Transform, enable: true);
			UIHelper.EnableGameObject(QuestRewardUI.gameObject, enable: true);
			List<MissionCategoryData> missionCategoryList = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(SelectedMainCategory);
			if (missionCategoryList != null && __Extension_Method_ICollection__.COUNT(missionCategoryList) != 0)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SubCategoryButtons); i++)
				{
					MissionCategoryData missionCategoryData = missionCategoryList[i];
					if (missionCategoryData != null)
					{
						int num = i + 1;
						SubCategoryButtons[i].OpenComponent(num, missionCategoryData, this);
						SubCategoryButtons[i].ToggleState(num == SelectedSubCategory);
					}
				}
			}
			if (SelectedMainCategory != 0 && SelectedSubCategory != 0)
			{
				MissionManagement.CheckNewMark(SelectedMainCategory, SelectedSubCategory);
			}
		}
		External_ChangeQuestListView();
		return true;
	}

	private void External_ChangeQuestListView()
	{
		if (!GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && SceneAutoBehaviour<MissionManagement>.isValidInstance)
		{
			MissionMain instance = SceneAutoBehaviour<MissionManagement>.instance.GetInstance<MissionMain>();
			if (!(instance == null))
			{
				instance.ChangedQuestListView();
			}
		}
	}

	private void SetupSubCategoryReward()
	{
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(SelectedMainCategory, SelectedSubCategory);
		if (data == null)
		{
			return;
		}
		string rewardQuestType = data.RewardQuestType;
		if (String_Extension_Method.IsNullOrEmpty(rewardQuestType))
		{
			return;
		}
		if (rewardQuestType.CompareTo("Achievement") == 0)
		{
			MissionComponentData elementData = new MissionComponentData(data.RewardQuestIndex);
			QuestRewardUI.SetupUI(elementData, __Extension_Method_ICollection__.COUNT(_ListClearedNodeIndex) > 0);
		}
		else if (rewardQuestType.CompareTo("SubQuest") == 0)
		{
			SubQuestData subQuestData = GameRoot.Instance.SubQuestDataContainer.GetSubQuestData(data.RewardQuestIndex);
			if (subQuestData != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(subQuestData);
				MissionComponentData elementData2 = new MissionComponentData(subQuestData);
				QuestRewardUI.SetupUI(elementData2, __Extension_Method_ICollection__.COUNT(_ListClearedNodeIndex) > 0);
			}
		}
	}

	private bool NeedUpdateDailyEventAchievement()
	{
		return false;
	}

	private void UpdateDailyEventAchievement()
	{
	}

	public void ForceUpdateDailyEventAchievement()
	{
	}

	private void SetQuestGridListCellHeight(int gridHeight)
	{
		if (ListGrid != null && (bool)ListGrid.Grid)
		{
			ListGrid.Grid.cellHeight = gridHeight;
		}
	}

	private void AddCompleteQuest(ref List<int> list, EQuestType questType, int index)
	{
		if (questType == EQuestType.Achievement)
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(index, 1);
			if (data != null && (data.Type == AchievementType.Daily || data.Type == AchievementType.Weekly) && (data.ReqType == AchievementReqType.CompleteDailyQuest || data.ReqType == AchievementReqType.CompleteWeeklyQuest))
			{
				list.Add(data.Index);
			}
		}
	}

	void MissionCategoryPanel.IListener.SelectCategoryMenu(MissionCategoryData data)
	{
		SelectMainCategory(data.MainCategoryIndex);
	}

	void MissionCategoryComponent.IListner.OnClick(MissionCategoryComponent comp)
	{
		SelectSubCategory(comp.MissionCategoryData.SubCategoryIndex);
	}

	void MissionComponent.IListner.OnClick(MissionComponentData data)
	{
		if (data.GetData<WebContentInfo>() != null)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OpenWebEvent(data.Index);
			GetMissionWindowManager()?.RefreshList();
		}
		else if (data.GetData<AttendanceData>() != null)
		{
			AttendanceKeywordGroup attendanceGroup = AttendanceKeywordGroup.Helper.CreateByAnyKeyword(data.GetData<AttendanceData>().Keyword);
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReopenAttendancePopup(attendanceGroup);
		}
		else if (data.GetData<BoosterInfo>() != null)
		{
			BoosterInfo data2 = data.GetData<BoosterInfo>();
			if (data2.MoveTaskType != ClientActivityType.None && !String_Extension_Method.IsNullOrEmpty(data2.MoveTaskType.ToString()) && (data2.MinLevel <= 0 || GlobalAutoBehaviour<AccountManager>.instance.HasMinLevelHero(data2.MinLevel)))
			{
				GetMissionWindowManager()?.Move(data2.MoveTaskType.ToString(), data2.MoveTaskValue);
			}
		}
		else
		{
			GetMissionWindowManager()?.OpenMissionPopup(data);
		}
	}

	void MissionComponent.IListner.RequestReward(MissionComponentData data)
	{
		RequestRewardInternal(data);
	}

	void MissionComponent.IListner.RequestMove(string moveTaskType, string[] moveTaskValue)
	{
		GetMissionWindowManager()?.Move(moveTaskType, moveTaskValue);
	}

	public void OnClickReward()
	{
		if (!(QuestRewardUI == null) && QuestRewardUI.ElementData != null)
		{
			RequestRewardInternal(QuestRewardUI.ElementData);
		}
	}

	private void RequestRewardInternal(MissionComponentData data)
	{
		CurrencyMaxCap.WarningOrExecute(CommonMission.GetCurrencyTypesFromReward(data), delegate
		{
			GetMissionWindowManager()?.RequestReward(data);
		});
	}

	public void OnClickAllGet()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_ListClearedNodeIndex))
		{
			return;
		}
		List<int> AchievementIndexList = new List<int>();
		List<int> SubQuestIndexList = new List<int>();
		HashSet<CurrencyType> hashSet = new HashSet<CurrencyType>();
		foreach (Tuple<EQuestType, int> item in _ListClearedNodeIndex)
		{
			if (!_DicMissionComponents.TryGetValue(item, out var value))
			{
				continue;
			}
			MissionComponentData elementData = value.ElementData;
			if (elementData == null)
			{
				continue;
			}
			if (elementData.GetData<AchievementData>() != null)
			{
				if (elementData.GetData<AchievementData>().RewardType == AchievementRewardType.Booster)
				{
					continue;
				}
				AchievementIndexList.Add(elementData.Index);
			}
			else if (elementData.GetData<SubQuestData>() != null)
			{
				SubQuestIndexList.Add(elementData.Index);
			}
			HashSet<CurrencyType> currencyTypesFromReward = CommonMission.GetCurrencyTypesFromReward(elementData);
			__Extension_Method_ICollection__.ADDRANGE(hashSet, currencyTypesFromReward);
		}
		CurrencyMaxCap.WarningOrExecute(hashSet, delegate
		{
			GetMissionWindowManager()?.Reward(AchievementIndexList, SubQuestIndexList);
		});
	}

	private void SetupList2RemainTime()
	{
		if (ListGridType != EListType.List2)
		{
			return;
		}
		MissionCategoryData data = GameRoot.Instance.MissionCategoryDataContainer.GetData(SelectedMainCategory, SelectedSubCategory);
		if (data != null)
		{
			if (data.IsWeeklyMission())
			{
				int remainWeekSecond = RequestEnterLobbyWrapper.GetRemainWeekSecond();
				UIHelper.SetRemainTimeLabelText(RemainingTimer, remainWeekSecond);
			}
			else if (data.IsDailyMission())
			{
				int remainDaySecond = RequestEnterLobbyWrapper.GetRemainDaySecond();
				UIHelper.SetRemainTimeLabelText(RemainingTimer, remainDaySecond);
			}
		}
	}

	private int GetTowerIndex(AchievementData achievementData)
	{
		int num = 0;
		if (achievementData == null)
		{
			return 0;
		}
		return achievementData.ReqType switch
		{
			AchievementReqType.LimitedClearOrdealTower => GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Ordeal), 
			AchievementReqType.LimitedClearChallengeTower => GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Challenge), 
			AchievementReqType.LimitedMazeClearRewardClearCheck => GameRoot.Instance.TowerDataContainer.GetTowerIndexByType(TowerType.Maze), 
			_ => 0, 
		};
	}

	private void SetRemainTime(AchievementData achievementData)
	{
		if (achievementData == null)
		{
			return;
		}
		if (__Extension_Method_ICollection__.COUNT(achievementData.MonthlyResetDate) > 0)
		{
			int remainMonthSecond = RequestEnterLobbyWrapper.GetRemainMonthSecond(achievementData.MonthlyResetDate);
			UIHelper.SetRemainTimeLabelText(RemainingTimer, remainMonthSecond);
		}
		else if (achievementData.WeeklyReset)
		{
			int remainWeekSecond = RequestEnterLobbyWrapper.GetRemainWeekSecond();
			UIHelper.SetRemainTimeLabelText(RemainingTimer, remainWeekSecond);
		}
		else if (achievementData.DailyReset)
		{
			int remainDaySecond = RequestEnterLobbyWrapper.GetRemainDaySecond();
			UIHelper.SetRemainTimeLabelText(RemainingTimer, remainDaySecond);
		}
		else if (achievementData.ReqType == AchievementReqType.LimitedClearChallengeRaid)
		{
			int remainTime = SceneAutoBehaviour<RaidManagement>.instance.GetRemainTime();
			long num = remainTime;
			int num2 = 0;
			if (num > num2)
			{
				UIHelper.SetLabelText(RemainingTimer, LocalizationManager.GetString(GuildRaid.GUILDRAID_STATE_OPENED, UIHelper.GetRemainTimeLabelText(num)));
			}
		}
		else if (achievementData.LimitedEventMissionType != EventBonusType.None)
		{
			int remainEventTime = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetRemainEventTime(achievementData.LimitedEventMissionType, achievementData.EventName);
			if (remainEventTime > 0)
			{
				UIHelper.SetRemainTimeLabelText(RemainingTimer, remainEventTime);
				return;
			}
			string stringDirect = LocalizationManager.GetStringDirect("EVENT_MISSION_NOTICE_NOTPERIOD");
			UIHelper.SetLabelText(RemainingTimer, stringDirect);
		}
	}

	public void SetupRemainTime(AchievementData achievementData)
	{
		int towerIndex = GetTowerIndex(achievementData);
		if (towerIndex > 0)
		{
			TowerHelper.SetupRemainResetTimeLabel(RemainingTimer, towerIndex);
		}
		else
		{
			SetRemainTime(achievementData);
		}
	}

	void WindowAnimation.IAlphaAnimator.SetAlpha(float alpha)
	{
		GetComponent<UIWidget>().alpha = alpha;
	}

	public string GetTopMostTitle()
	{
		List<MissionCategoryData> missionCategoryList = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryList(SelectedMainCategory);
		if (missionCategoryList != null)
		{
			using List<MissionCategoryData>.Enumerator enumerator = missionCategoryList.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current.MainCategoryLocalKey;
			}
		}
		return "CONTENT_MISSION";
	}
}
