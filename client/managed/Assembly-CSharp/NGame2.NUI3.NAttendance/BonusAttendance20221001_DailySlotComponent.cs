using System;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI3.NAttendance;

public class BonusAttendance20221001_DailySlotComponent : UIScrollItemBehaviour, InitOnCreate
{
	[Serializable]
	public class DailyRewardUi : CurrencyIconComponent.Listener, ItemIconComponent.Listener
	{
		public UIWidget Widget_Pivot;

		public UIWidget Widget_when_AlreadyReceived;

		public UIWidget Widget_when_CanReceive;

		public UIWidget Widget_when_Locked;

		private bool _isMain;

		private int _day;

		private BonusAttendance20221001_AttendanceWindow _myAttendanceWindow;

		private Component _rewardComponent;

		public BonusAttendance20221001_Facade.ERewardState CurrentRewardState { get; private set; }

		public void InitUi(bool isMain, int day, BonusAttendance20221001_AttendanceWindow myAttendanceWindow)
		{
			_isMain = isMain;
			_day = day;
			_myAttendanceWindow = myAttendanceWindow;
			ResetUi();
			if (!(Widget_Pivot == null))
			{
				AttendanceReward attendanceReward = BonusAttendance20221001_Facade.GetAttendanceReward(isMain, day);
				if (attendanceReward != null)
				{
					CurrentRewardState = BonusAttendance20221001_Facade.GetRewardState_by_Day(isMain, day);
					UIHelper.EnableGameObject(Widget_when_AlreadyReceived, CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.Already_Received);
					UIHelper.EnableGameObject(Widget_when_CanReceive, CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.CanReceive);
					UIHelper.EnableGameObject(Widget_when_Locked, CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.Locked);
					bool isGrayscale = CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.Locked;
					_rewardComponent = AttendanceHelper.CreateRewardComponent(this, attendanceReward, isGrayscale, Widget_Pivot.transform);
				}
			}
		}

		public void RefreshUi()
		{
			BonusAttendance20221001_Facade.ERewardState rewardState_by_Day = BonusAttendance20221001_Facade.GetRewardState_by_Day(_isMain, _day);
			if (CurrentRewardState != rewardState_by_Day)
			{
				InitUi(_isMain, _day, _myAttendanceWindow);
			}
		}

		public void ResetUi()
		{
			FactoryBase.SafeReleaseComponent(ref _rewardComponent);
		}

		void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
		{
			if (CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.CanReceive && _myAttendanceWindow != null)
			{
				_myAttendanceWindow.OnClick_RewardComponent_from_DailySlotComponent();
			}
		}

		void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
		{
			if (CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.CanReceive && _myAttendanceWindow != null)
			{
				_myAttendanceWindow.OnClick_RewardComponent_from_DailySlotComponent();
			}
		}
	}

	public const string PrefabPath = "Prefab3/Attendance/BonusAttendance20221001_DailySlotComponent";

	public static PrefabAuto<BonusAttendance20221001_DailySlotComponent> PrefabAuto = new PrefabAuto<BonusAttendance20221001_DailySlotComponent>("Prefab3/Attendance/BonusAttendance20221001_DailySlotComponent");

	[Header("-- [ 출석 날짜 ]")]
	public UILabel Label_Day;

	[Header("-- [ 첫번째 보너스 출석 보상 ]")]
	public DailyRewardUi DailyReward_First;

	[Header("-- [ 두번째 보너스 출석 보상 ]")]
	public DailyRewardUi DailyReward_Second;

	public void InitUi(BonusAttendance20221001_AttendanceWindow myAttendanceWindow, int day)
	{
		UIHelper.SetLabelText(Label_Day, NGame2.NUI.Common.GetOrdinalizeDay(day));
		DailyReward_First.InitUi(isMain: true, day, myAttendanceWindow);
		DailyReward_Second.InitUi(isMain: false, day, myAttendanceWindow);
	}

	public void RefreshUi()
	{
		DailyReward_First.RefreshUi();
		DailyReward_Second.RefreshUi();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	public void OpenDailySlot()
	{
	}

	private void ReleaseComponent()
	{
		DailyReward_First.ResetUi();
		DailyReward_Second.ResetUi();
	}
}
