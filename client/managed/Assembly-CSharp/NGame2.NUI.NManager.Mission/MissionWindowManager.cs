using System;
using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NContentsDefine;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.Mission;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.Mission;

public class MissionWindowManager : MissionMain.IListener, MainQuestGuideMenu.IListener, MissionMilestonePopup.Listener, CommonNodeWindow.IListner
{
	public bool IgnoreCloseNextStep;

	private const string DEFAULT_NPC_Code = "Mission_Milestone";

	private MissionManagement _management;

	public void Init(MissionManagement t)
	{
		_management = t;
	}

	public void OpenMissionMain(int mainCategory = 0, int subCategory = 0, bool forceOpen = false)
	{
		if (_management == null)
		{
			return;
		}
		bool flag = false;
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				flag = true;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				flag = true;
			}
		}
		if (flag)
		{
			TutorialSkipPopup tutorialSkipPopup = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<TutorialSkipPopup>();
			if (tutorialSkipPopup != null)
			{
				flag = tutorialSkipPopup.IsOpen();
			}
		}
		if (!flag && !forceOpen && (!SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty() || SceneAutoBehaviour<LobbyPopupQueue>.instance.IsProgressing()))
		{
			return;
		}
		MissionMain instance = _management.GetInstance<MissionMain>();
		if (instance == null)
		{
			return;
		}
		IgnoreCloseNextStep = true;
		GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows(immediately: true);
		IgnoreCloseNextStep = false;
		instance.OpenMain(delegate
		{
			if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<EquipAwakenScene>())
			{
				EquipAwakenScene window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<EquipAwakenScene>();
				if (window != null)
				{
					window.Close2(immediately: true);
				}
			}
		}, null, this, "Mission_Milestone", mainCategory, subCategory);
	}

	public void OpenMissionNodeWindow(int mainCategory, int subCategory = 0)
	{
		if (!(_management == null))
		{
			MissionNodeWindow instance = _management.GetInstance<MissionNodeWindow>();
			if (!(instance == null))
			{
				MissionNodeWindowTopView2 instance2 = _management.GetInstance<MissionNodeWindowTopView2>();
				instance.OpenNodeWindow(mainCategory, subCategory, this, instance2);
			}
		}
	}

	public void OpenMissionNodeWindowV3(int mainCategory, int subCategory = 0)
	{
		if (!(_management == null))
		{
			MissionNodeWindowV3 instance = _management.GetInstance<MissionNodeWindowV3>();
			if (!(instance == null))
			{
				MissionNodeWindowTopView2 instance2 = _management.GetInstance<MissionNodeWindowTopView2>();
				instance.OpenNodeWindow(mainCategory, subCategory, this, instance2);
			}
		}
	}

	public void OpenMissionNode2Window(int mainCategory, int subCategory = 0)
	{
		if (!(_management == null))
		{
			MissionNode2Window instance = _management.GetInstance<MissionNode2Window>();
			if (!(instance == null))
			{
				MissionNodeWindowTopView2 instance2 = _management.GetInstance<MissionNodeWindowTopView2>();
				instance.OpenNodeWindow(mainCategory, subCategory, this, instance2);
			}
		}
	}

	public void OpenMissionPopup(MissionComponentData elementData, bool Completed = false, bool Available = true)
	{
		if (elementData != null)
		{
			bool flag = true;
			if (elementData.VisualData != null)
			{
				flag = String_Extension_Method.IsNullOrEmpty(elementData.VisualData.MilestonePopupNpcCode);
			}
			if (!SingletonUpdater<TutorialManager>.instance.IsProgressing && flag)
			{
				MissionMilestonePopup.OpenGlobal(this, elementData, Completed, Available);
			}
			else
			{
				MissionMilestonePopup.OpenNormal(this, elementData, Completed, Available);
			}
		}
	}

	public void Move(string moveTaskType, string[] moveTaskValue)
	{
		if (!(_management == null))
		{
			CloseMissionWnds();
			_management.Move(moveTaskType, moveTaskValue);
		}
	}

	public void Reward(Type dataType, List<int> indexList, Action callbackAction, float openDelayTime = 0f)
	{
		if (!(dataType == null))
		{
			if (dataType == typeof(AchievementData))
			{
				RequestAchievementReward(indexList, (callbackAction == null) ? new Action(ResponseReward) : callbackAction, openDelayTime);
			}
			else if (dataType == typeof(SubQuestData))
			{
				RequestSubQuestReward(indexList, callbackAction, openDelayTime);
			}
		}
	}

	public void Reward(List<int> achievementList, List<int> subQuestList, float openDelayTime = 0f)
	{
		if (__Extension_Method_ICollection__.COUNT(achievementList) > __Extension_Method_ICollection__.COUNT(subQuestList))
		{
			RequestAchievementReward(achievementList, ResponseReward, openDelayTime);
		}
		else
		{
			RequestSubQuestReward(subQuestList, ResponseReward, openDelayTime);
		}
	}

	private void RequestSubQuestReward(List<int> IndexList, Action callbackAction = null, float openDelayTime = 0f)
	{
		if (!(_management == null))
		{
			_management.RequestCompleteSubQuest(IndexList, callbackAction, openDelayTime);
		}
	}

	private void RequestAchievementReward(List<int> indexList, Action callbackAction, float openDelayTime = 0f)
	{
		if (!(_management == null))
		{
			_management.RequestAchievementReward(indexList, callbackAction, openDelayTime);
		}
	}

	private void ResponseReward()
	{
		RefreshList();
		if (!(_management == null))
		{
			_management.ResponseReward();
		}
	}

	public void RefreshList()
	{
		if (!(_management == null))
		{
			_management.RefreshQuestList();
			MissionNodeWindow instance = _management.GetInstance<MissionNodeWindow>();
			if (instance != null && instance.IsOpen())
			{
				instance.Refresh();
			}
			MissionNode2Window instance2 = _management.GetInstance<MissionNode2Window>();
			if (instance2 != null && instance2.IsOpen())
			{
				instance2.Refresh();
			}
			MissionNodeWindowV3 instance3 = _management.GetInstance<MissionNodeWindowV3>();
			if (instance3 != null && instance3.IsOpen())
			{
				instance3.Refresh();
			}
		}
	}

	public void CloseMissionWnds()
	{
		if (_management == null)
		{
			return;
		}
		MissionNodeWindow instance = _management.GetInstance<MissionNodeWindow>();
		if (instance != null && instance.IsOpen())
		{
			instance.Close();
		}
		MissionNode2Window instance2 = _management.GetInstance<MissionNode2Window>();
		if (instance2 != null && instance2.IsOpen())
		{
			instance2.Close();
		}
		MissionNodeWindowV3 instance3 = _management.GetInstance<MissionNodeWindowV3>();
		if (instance3 != null && instance3.IsOpen())
		{
			instance3.Close();
		}
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<AchievementList>())
		{
			AchievementList window = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<AchievementList>();
			if (window != null)
			{
				window.Close2(immediately: true);
			}
		}
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<HeroAchievementList>())
		{
			HeroAchievementList window2 = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<HeroAchievementList>();
			if (window2 != null)
			{
				window2.Close2(immediately: true);
			}
		}
		MissionMilestonePopup.ReleaseWindow();
		SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		MissionMain instance4 = _management.GetInstance<MissionMain>();
		if (instance4 != null)
		{
			instance4.Close();
		}
	}

	public void CloseMissionMainSubWnds()
	{
		if (!(_management == null))
		{
			MissionMain instance = _management.GetInstance<MissionMain>();
			if (!(instance == null) && !(instance.MissionListPanel == null) && !(instance.MissionListPanel.QuestListPanel == null))
			{
				instance.SelectDefaultCategory();
				string topMostTitle = instance.MissionListPanel.QuestListPanel.GetTopMostTitle();
				instance.title.Key = topMostTitle;
			}
		}
	}

	void MissionMilestonePopup.Listener.Reward(MissionComponentData data, Action complete, float openDelayTime)
	{
		List<int> list = new List<int>();
		list.Add(data.Index);
		Reward(data.DataType, list, complete, openDelayTime);
	}

	void MissionMilestonePopup.Listener.Move(string moveTaskType, string[] moveTaskValue, AchievementReqType type)
	{
		if (type == AchievementReqType.ClearHardRaid && !SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.Raid))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("FAILED_MISSION_MOVE_NOT_OPEN_RAID"));
			return;
		}
		SceneAutoBehaviour<WorldMapManagement>.instance.IsPossibleMove = true;
		Move(moveTaskType, moveTaskValue);
	}

	public void CloseMilestonePopup(bool immediately, Action onComplete)
	{
		MissionMilestonePopup missionMilestonePopup = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<MissionMilestonePopup>();
		if (missionMilestonePopup == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (immediately)
		{
			missionMilestonePopup.Close();
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else
		{
			missionMilestonePopup.CloseWithAnimation(onComplete);
		}
	}

	public void RequestReward(MissionComponentData data)
	{
		List<int> list = new List<int>();
		list.Add(data.Index);
		Reward(data.DataType, list, ResponseReward);
	}

	void MainQuestGuideMenu.IListener.Move(string moveTaskType, string[] moveTaskValue)
	{
		Move(moveTaskType, moveTaskValue);
	}

	void CommonNodeWindow.IListner.OnClickNode(MissionComponentData data)
	{
		OpenMissionPopup(data);
	}

	void MissionMilestonePopup.Listener.ResponseReward()
	{
		RefreshList();
		if (_management != null)
		{
			_management.ResponseReward();
		}
	}

	void CommonNodeWindow.IListner.RequestReward(MissionComponentData data)
	{
		List<int> list = new List<int>();
		list.Add(data.Index);
		Reward(data.DataType, list, ResponseReward);
	}

	void CommonNodeWindow.IListner.RequestAllGetReward(List<int> achievementList, List<int> subQuestList)
	{
		CurrencyMaxCap.WarningOrExecuteForAll(delegate
		{
			Reward(achievementList, subQuestList);
		});
	}

	void CommonNodeWindow.IListner.CloseNodeWindow()
	{
		CloseMissionMainSubWnds();
	}

	void CommonNodeWindow.IListner.EventRemainTimeOut()
	{
		MissionMilestonePopup.ReleaseWindow();
	}
}
