using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NLogin;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LeftMenu : BaseWindow, IUserObserver<SubQuestInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo>, IUserObserver<AttendanceInfo>, IBoosterListObserver, LeftMenuMissionProgress.Listner, ButtonSpecialEventComponent.Listner
{
	public interface Listener
	{
		void OnClickQuestButton(EMissionType missionType);

		void OnClickAttendanceButton();

		void OnClickMissionButton(MissionComponentData data);

		void OnClickSpecialMissionButton(MissionCategoryData data);
	}

	[Serializable]
	public class LeftBottomGroup
	{
		public UITable Table;

		public UIScrollView ScrollView;

		private List<Component> _componentList = new List<Component>();

		public void OnStart()
		{
			if (Table != null)
			{
				Table.onCustomSort = ComponentTableSort;
			}
		}

		public void Init()
		{
			string serverName = SingletonUpdater<LoginManager>.instance.ServerName;
			ContentsDefine.OfficialChannel.GetSetting(serverName, out var _);
			InitLeftMenuEventIconComponent();
		}

		private void InitLeftMenuEventIconComponent()
		{
			List<LeftMenuButtonData> sortedList = GameRoot.Instance.LeftMenuButtonDataContainer.GetSortedList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(sortedList); i++)
			{
				LeftMenuButtonData leftMenuButtonData = sortedList[i];
				if (leftMenuButtonData != null)
				{
					LeftMenuEventIconComponent leftMenuEventIconComponent = LeftMenuEventIconComponent.CreateEventIcon(leftMenuButtonData, Table, ScrollView);
					if (!(leftMenuEventIconComponent == null))
					{
						_componentList.Add(leftMenuEventIconComponent);
					}
				}
			}
		}

		public void ReleaseAllComponent()
		{
			FactoryBase.ReleaseAndClearList(_componentList);
		}

		public void Update()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
			{
				Component component = _componentList[i];
				if (!(component == null))
				{
					LeftMenuEventIconComponent leftMenuEventIconComponent = component as LeftMenuEventIconComponent;
					if (leftMenuEventIconComponent != null)
					{
						leftMenuEventIconComponent.UpdateEvent();
					}
				}
			}
			Table.Reposition();
		}

		private int ComponentTableSort(Transform a, Transform b)
		{
			int num = 0;
			num = NullCompare(a, b);
			if (num != 0)
			{
				return num;
			}
			LeftMenuEventIconComponent component = a.GetComponent<LeftMenuEventIconComponent>();
			LeftMenuEventIconComponent component2 = b.GetComponent<LeftMenuEventIconComponent>();
			num = NullCompare(component, component2);
			if (num != 0)
			{
				return -num;
			}
			if (component == null)
			{
				return 0;
			}
			LeftMenuButtonData leftMenuButtonData = component.LeftMenuButtonData;
			LeftMenuButtonData leftMenuButtonData2 = component2.LeftMenuButtonData;
			num = NullCompare(leftMenuButtonData, leftMenuButtonData2);
			if (num != 0)
			{
				return num;
			}
			return leftMenuButtonData.ShowIndex.CompareTo(leftMenuButtonData2.ShowIndex);
		}
	}

	[Serializable]
	public class AttendanceGroupUi
	{
		public UIButton Button_Attendance;

		public UIWidget Widget_CanReceiveReward;

		public void InitUi()
		{
			bool enable = AttendanceFacade.CanReceiveReward_Any();
			UIHelper.EnableGameObject(Widget_CanReceiveReward, enable);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/LeftMenu";

	public LeftBottomGroup BottomGroup;

	public UIGrid SpecialEventGrid;

	public int MaxSpecialEventNum = 3;

	public LeftMenuMissionProgress MissionProgress;

	public Animator EventButtonAnimator;

	public UISprite EventButtonRewardSprite;

	public UISprite EventButtonNewSprite;

	public UIButton ButtonAttendance;

	[Header("-- [ 출석 버튼 ]")]
	public AttendanceGroupUi AttendanceGroup;

	private float _lastUpdateTime;

	private List<ButtonSpecialEventComponent> _specialEventButtonComponentList = new List<ButtonSpecialEventComponent>();

	private Listener _listener;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IBoosterListObserver.OnChanged(BoosterInfo expiredBooster)
	{
		SetFixedButton();
	}

	void IUserObserver<SubQuestInfo>.OnChanged(SubQuestInfo changedInfo)
	{
		if (MissionProgress != null)
		{
			MissionProgress.OnChanged(changedInfo);
		}
		Refresh();
	}

	void IUserObserver<AttendanceInfo>.OnChanged(AttendanceInfo changedInfo)
	{
		AttendanceGroup.InitUi();
	}

	void IUserObserver<NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo>.OnChanged(NGame2.NAccount.UserAchievementManager.ChangeAchievementInfo changedInfo)
	{
		if (MissionProgress != null)
		{
			MissionProgress.OnChanged(changedInfo);
		}
		Refresh();
	}

	private void Start()
	{
		BottomGroup.OnStart();
		if ((bool)SpecialEventGrid)
		{
			SpecialEventGrid.onCustomSort = RightMenuButtonGridSort;
		}
	}

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<BoosterManager>.instance.AddObserver(this);
		Init();
	}

	private void Init()
	{
		SetFixedButton();
		SetRightMenuButton();
		BottomGroup.Init();
		AttendanceGroup.InitUi();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (GlobalAutoBehaviour<BoosterManager>.isValidInstance)
			{
				GlobalAutoBehaviour<BoosterManager>.instance.RemoveObserver(this);
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.RemoveObserver(this);
			}
			_listener = null;
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (MissionProgress != null)
		{
			MissionProgress.Clear();
		}
		ClearRightMenuButton();
		BottomGroup.ReleaseAllComponent();
	}

	private void ClearRightMenuButton()
	{
		FactoryBase.ReleaseAndClearList(_specialEventButtonComponentList);
	}

	public void Refresh()
	{
		if (MissionProgress != null)
		{
			MissionProgress.Refresh();
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public void RepositionSpecialEventTable()
	{
		if (SpecialEventGrid != null)
		{
			SpecialEventGrid.Reposition();
		}
	}

	public void SetFixedButton()
	{
		if (MissionProgress != null)
		{
			MissionProgress.Init(this);
		}
		SetupButtons();
		RepositionSpecialEventTable();
	}

	private void SetupButtons()
	{
		bool flag = MissionManagement.IsNewEventCategory();
		UIHelper.EnableGameObject(EventButtonNewSprite, flag);
		if (!flag)
		{
			bool flag2 = MissionManagement.IsMissionEventCategoryRewardCheck();
			if (flag2 && EventButtonAnimator != null)
			{
				EventButtonAnimator.Play("Anim_Button_Event_Reward");
			}
			UIHelper.EnableGameObject(EventButtonRewardSprite, flag2);
		}
	}

	public void SetRightMenuButton()
	{
		if (__Extension_Method_ICollection__.COUNT(_specialEventButtonComponentList) > 0)
		{
			ClearRightMenuButton();
		}
		IList<MissionCategoryData> allData = GameRoot.Instance.MissionCategoryDataContainer.GetAllData();
		int num = 0;
		foreach (MissionCategoryData item in allData)
		{
			if ((!item.EqualMissionType(EMissionType.Special) && !item.EqualMissionType(EMissionType.Special2) && !item.EqualMissionType(EMissionType.Special3)) || String_Extension_Method.IsNullOrEmpty(item.ButtonPrefabName) || !MissionManagement.IsOpenSubCategory(item))
			{
				continue;
			}
			ButtonSpecialEventComponent buttonSpecialEventComponent = ButtonSpecialEventComponent.CreateComponent(item.ButtonPrefabName, SpecialEventGrid.transform);
			if (!(buttonSpecialEventComponent == null))
			{
				buttonSpecialEventComponent.Open(item, this);
				_specialEventButtonComponentList.Add(buttonSpecialEventComponent);
				if (++num >= MaxSpecialEventNum)
				{
					break;
				}
			}
		}
		RepositionSpecialEventTable();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!(Time.time - _lastUpdateTime >= 1f))
		{
			return;
		}
		_lastUpdateTime = Time.time;
		BottomGroup.Update();
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_specialEventButtonComponentList); i++)
		{
			ButtonSpecialEventComponent buttonSpecialEventComponent = _specialEventButtonComponentList[i];
			if (!(buttonSpecialEventComponent == null) && buttonSpecialEventComponent.IsTimeout())
			{
				_specialEventButtonComponentList.RemoveAt(i);
				i--;
				FactoryBase.ReleaseComponent(buttonSpecialEventComponent);
				flag = true;
			}
		}
		if (flag)
		{
			RepositionSpecialEventTable();
		}
	}

	public void OnEnterLobby()
	{
		SetFixedButton();
		SetRightMenuButton();
	}

	public static int RightMenuButtonGridSort(Transform a, Transform b)
	{
		int num = 0;
		num = NullCompare(a, b);
		if (num != 0)
		{
			return num;
		}
		ButtonSpecialEventComponent component = a.GetComponent<ButtonSpecialEventComponent>();
		ButtonSpecialEventComponent component2 = b.GetComponent<ButtonSpecialEventComponent>();
		num = NullCompare(component, component2);
		if (num != 0)
		{
			return num;
		}
		MissionCategoryData missionCategoryData = component.MissionCategoryData;
		MissionCategoryData missionCategoryData2 = component2.MissionCategoryData;
		num = NullCompare(missionCategoryData, missionCategoryData2);
		if (num != 0)
		{
			return num;
		}
		return missionCategoryData.MainCategoryIndex.CompareTo(missionCategoryData2.MainCategoryIndex);
	}

	public static int NullCompare(object a, object b)
	{
		if ((a == null && b == null) || (a != null && b != null))
		{
			return 0;
		}
		if (b != null)
		{
			return 1;
		}
		return -1;
	}

	public void OnClickEventButton()
	{
		if (_listener != null)
		{
			_listener.OnClickQuestButton(EMissionType.Event);
		}
	}

	public void OnClickAttendanceButton()
	{
		if (_listener != null)
		{
			_listener.OnClickAttendanceButton();
		}
	}

	void LeftMenuMissionProgress.Listner.OnClickMission(MissionComponentData data)
	{
		if (_listener != null)
		{
			_listener.OnClickMissionButton(data);
		}
	}

	void LeftMenuMissionProgress.Listner.OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClickQuestButton(EMissionType.Null);
		}
	}

	void ButtonSpecialEventComponent.Listner.OnClick(MissionCategoryData data)
	{
		if (_listener != null)
		{
			EMissionType missionType = data.MissionType;
			if ((uint)(missionType - 8) <= 2u)
			{
				_listener.OnClickSpecialMissionButton(data);
			}
		}
	}
}
