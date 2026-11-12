using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NUI;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI3.NCommon;
using NShared;
using NShared.GetAllConditionalAttendanceReward;
using NShared.GetAttendanceReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI3.NAttendance;

public sealed class BonusAttendance20221001_AttendanceWindow : AttendanceWindow, IUserObserver<AttendanceInfo>, IUserObserver
{
	[Serializable]
	public class LuaPointUi
	{
		public int MiniGuideData_Index;

		public UILocalizeEx LocalEx_Announce;

		public UIClipSprite ClipSprite_Gauge;

		public UILabel Label_LuaPoint;

		public void InitUi()
		{
			int require_LuaPoint_from_AttendanceInfoData = BonusAttendance20221001_Facade.GetRequire_LuaPoint_from_AttendanceInfoData(isMain: false);
			int monthlyAcquiredLuaPoint = BonusAttendance20221001_Facade.GetMonthlyAcquiredLuaPoint();
			float fillAmount = Mathf.Min((float)monthlyAcquiredLuaPoint / (float)Mathf.Max(1, require_LuaPoint_from_AttendanceInfoData), 1f);
			if (LocalEx_Announce != null)
			{
				BonusAttendance20221001_Facade.GetAttendancePeriod(out var beginDate, out var endDate);
				string arg = beginDate.ToString("yyyy/MM/dd");
				string arg2 = endDate.ToString("yyyy/MM/dd");
				LocalEx_Announce.SetLocalizeArg($"{arg} ~ {arg2}");
			}
			UIHelper.SetSpriteFillAmount(ClipSprite_Gauge, fillAmount);
			if (monthlyAcquiredLuaPoint >= require_LuaPoint_from_AttendanceInfoData)
			{
				UIHelper.SetLabelText(Label_LuaPoint, "MAX");
			}
			else
			{
				UIHelper.SetLabelText(Label_LuaPoint, $"{monthlyAcquiredLuaPoint}/{require_LuaPoint_from_AttendanceInfoData}");
			}
		}
	}

	[Serializable]
	public class Requirement_LuaPointUi
	{
		public UILabel Label_Step1_LuaPoint;

		public UILabel Label_Step2_LuaPoint;

		public void InitUi()
		{
			int require_LuaPoint_from_AttendanceInfoData = BonusAttendance20221001_Facade.GetRequire_LuaPoint_from_AttendanceInfoData(isMain: true);
			int require_LuaPoint_from_AttendanceInfoData2 = BonusAttendance20221001_Facade.GetRequire_LuaPoint_from_AttendanceInfoData(isMain: false);
			UIHelper.SetLabelText(Label_Step1_LuaPoint, $"{require_LuaPoint_from_AttendanceInfoData}");
			UIHelper.SetLabelText(Label_Step2_LuaPoint, $"{require_LuaPoint_from_AttendanceInfoData2}");
		}
	}

	[Serializable]
	public class DailyRewardSlotUi
	{
		public UIScrollGrid ScrollGrid_DailySlot;

		private List<BonusAttendance20221001_DailySlotComponent> _componentList = new List<BonusAttendance20221001_DailySlotComponent>();

		public void InitUi(BonusAttendance20221001_AttendanceWindow myAttendanceWindow)
		{
			BonusAttendance20221001_DailySlotComponent bonusAttendance20221001_DailySlotComponent = null;
			int attendanceRewardEndDay = BonusAttendance20221001_Facade.GetAttendanceRewardEndDay();
			for (int i = 1; i <= attendanceRewardEndDay; i++)
			{
				BonusAttendance20221001_DailySlotComponent bonusAttendance20221001_DailySlotComponent2 = UIHelper.AddGridItem(ScrollGrid_DailySlot, BonusAttendance20221001_DailySlotComponent.PrefabAuto);
				bonusAttendance20221001_DailySlotComponent2.InitUi(myAttendanceWindow, i);
				_componentList.Add(bonusAttendance20221001_DailySlotComponent2);
				if (bonusAttendance20221001_DailySlotComponent == null && bonusAttendance20221001_DailySlotComponent2.DailyReward_First.CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.CanReceive)
				{
					bonusAttendance20221001_DailySlotComponent = bonusAttendance20221001_DailySlotComponent2;
				}
			}
			ScrollGrid_DailySlot.RepositionGrid();
			ScrollTo_TargetRewardComponent();
		}

		public void RefreshUi()
		{
			foreach (BonusAttendance20221001_DailySlotComponent component in _componentList)
			{
				component.RefreshUi();
			}
			ScrollTo_TargetRewardComponent();
		}

		public void ResetUi()
		{
			FactoryBase.ReleaseAndClearList(_componentList);
		}

		private void ScrollTo_TargetRewardComponent()
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_componentList))
			{
				return;
			}
			BonusAttendance20221001_DailySlotComponent bonusAttendance20221001_DailySlotComponent = null;
			BonusAttendance20221001_DailySlotComponent bonusAttendance20221001_DailySlotComponent2 = null;
			foreach (BonusAttendance20221001_DailySlotComponent component in _componentList)
			{
				if (component.DailyReward_First.CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.CanReceive && bonusAttendance20221001_DailySlotComponent == null)
				{
					bonusAttendance20221001_DailySlotComponent = component;
				}
				if (component.DailyReward_First.CurrentRewardState == BonusAttendance20221001_Facade.ERewardState.Already_Received)
				{
					bonusAttendance20221001_DailySlotComponent2 = component;
				}
			}
			if (bonusAttendance20221001_DailySlotComponent != null)
			{
				ScrollGrid_DailySlot.ScrollTo(bonusAttendance20221001_DailySlotComponent, 0f, 0f);
			}
			else if (bonusAttendance20221001_DailySlotComponent2 != null)
			{
				ScrollGrid_DailySlot.ScrollTo(bonusAttendance20221001_DailySlotComponent2, 0f, 0f);
			}
		}
	}

	[Serializable]
	public class LockByLuaPointUi
	{
		public UIBasicSprite Sprite_Lock_Step1;

		public UILocalizeEx LocalEx_Lock_Step1;

		public UIBasicSprite Sprite_Lock_Step2;

		public UILocalizeEx LocalEx_Lock_Step2;

		public void InitUi()
		{
			if (LocalEx_Lock_Step1 != null)
			{
				int require_LuaPoint_from_AttendanceInfoData = BonusAttendance20221001_Facade.GetRequire_LuaPoint_from_AttendanceInfoData(isMain: true);
				LocalEx_Lock_Step1.SetLocalizeArg(require_LuaPoint_from_AttendanceInfoData);
			}
			if (LocalEx_Lock_Step2 != null)
			{
				int require_LuaPoint_from_AttendanceInfoData2 = BonusAttendance20221001_Facade.GetRequire_LuaPoint_from_AttendanceInfoData(isMain: false);
				LocalEx_Lock_Step2.SetLocalizeArg(require_LuaPoint_from_AttendanceInfoData2);
			}
			bool flag = BonusAttendance20221001_Facade.IsUnlocked_by_LuaPoint(isMain: true);
			bool flag2 = BonusAttendance20221001_Facade.IsUnlocked_by_LuaPoint(isMain: false);
			UIHelper.EnableGameObject(Sprite_Lock_Step1, !flag);
			UIHelper.EnableGameObject(LocalEx_Lock_Step1, !flag);
			UIHelper.EnableGameObject(Sprite_Lock_Step2, !flag2);
			UIHelper.EnableGameObject(LocalEx_Lock_Step2, !flag2);
		}
	}

	[Serializable]
	public class ReceiveAllButtonUi
	{
		public UIButton Button;

		public void InitUi()
		{
			bool enable = BonusAttendance20221001_Facade.CanReceiveReward_Any();
			UIHelper.EnableButton(Button, enable);
		}
	}

	public const string PrefabPath = "Resources/Prefab3/Attendance/BonusAttendance20221001_AttendanceWindow";

	[Header("-- [ 우측 상단의 닫기 버튼 (출석 모아보기 에서 감춘다) ]")]
	public UIButton Button_Close;

	[Header("-- [ 상단의 루아 포인트 표기 및 설명 ]")]
	public LuaPointUi LuaPoint;

	[Header("-- [ 좌측의 필요한 루아 포인트 획득량 ]")]
	public Requirement_LuaPointUi Requirement_LuaPoint;

	[Header("-- [ 매일매일 보상 표기 ]")]
	public DailyRewardSlotUi DailyRewardSlot;

	[Header("-- [ 루아 포인트에 의한 잠금 표시 (BlackScreening) ]")]
	public LockByLuaPointUi LockByLuaPoint;

	[Header("-- [ 모두 받기 버튼 ]")]
	public ReceiveAllButtonUi ReceiveAllButton;

	[Header("-- [ 모두 받기 팝업 알림 문구 ]")]
	public LocalizeLinkDataInner LocalLink_PopupMessage_ReceiveAll;

	private AttendanceKeywordGroup _attendanceGroup;

	public sealed override void OpenAttendance(AttendanceKeywordGroup attendanceGroup, bool openedFromReservePopup, Action onComplete = null)
	{
		if (openedFromReservePopup)
		{
			BonusAttendance20221001_Facade.Enable_ReservePopup.Value = false;
		}
		Open(delegate
		{
			InitWindow(attendanceGroup);
			if (BonusAttendance20221001_Facade.DoesNeed_Request_for_NextAttendanceDay(isMain: true))
			{
				BonusAttendance20221001_Facade.Request_for_NextAttendanceDay(delegate
				{
					RefreshWindow();
				});
			}
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	public override void RefreshAttendance(AttendanceKeywordGroup attendanceGroup)
	{
		RefreshWindow();
	}

	private void InitWindow(AttendanceKeywordGroup attendanceGroup)
	{
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.AddObserver(this);
		_attendanceGroup = attendanceGroup;
		LuaPoint.InitUi();
		Requirement_LuaPoint.InitUi();
		DailyRewardSlot.InitUi(this);
		LockByLuaPoint.InitUi();
		ReceiveAllButton.InitUi();
	}

	private void RefreshWindow()
	{
		LuaPoint.InitUi();
		DailyRewardSlot.RefreshUi();
		LockByLuaPoint.InitUi();
		ReceiveAllButton.InitUi();
	}

	public sealed override void Setup_for_AttendanceIntegration()
	{
		UIHelper.EnableGameObject(Button_Close, enable: false);
	}

	public sealed override void ShowPurchaseButton(bool isShow)
	{
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
		}
	}

	public override void Reset()
	{
		base.Reset();
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.ObserverManager.RemoveObserver(this);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<AttendanceInfo>.OnChanged(AttendanceInfo changedInfo)
	{
		if (_attendanceGroup.IsAny_AttendanceIndex(changedInfo.AttendanceIndex))
		{
			RefreshWindow();
		}
	}

	public void OnClick_RewardComponent_from_DailySlotComponent()
	{
		if (BonusAttendance20221001_Facade.CanReceiveReward_Any())
		{
			YesNoPopup.Open(LocalLink_PopupMessage_ReceiveAll.GetLocalize(), delegate
			{
				OnClick_ReceiveAll();
			});
		}
	}

	public void OnClick_Close()
	{
		Close();
	}

	public void OnClick_ReceiveAll()
	{
		NShared.GetAllConditionalAttendanceReward.Request request = new NShared.GetAllConditionalAttendanceReward.Request();
		request.AttendanceIndex = new int[2] { _attendanceGroup.IndexMain, _attendanceGroup.IndexSub };
		NShared.GetAllConditionalAttendanceReward.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.GetAttendanceReward.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetAttendanceReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				DbRewardResult attendanceRewardResult = response.AttendanceRewardResult;
				if (attendanceRewardResult != null)
				{
					string stringDirect = LocalizationManager.GetStringDirect("UI_BONUS_ATTENDANCE");
					GlobalAutoBehaviour<AccountManager>.instance.ApplyDbRewardResult(attendanceRewardResult);
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetItemResultInfos(attendanceRewardResult.ItemResults).SetEquipResultInfos(attendanceRewardResult.EquipItemResults)
						.SetCurrencyResultInfos3(attendanceRewardResult.CurrencyResults)
						.AddPetInfos(attendanceRewardResult.PetInfos), isGlobal: false);
				}
				GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(response.AttendanceInfos);
			}
		});
	}

	public void OnClick_GameGuide_LuaPoint()
	{
		GameMiniGuideData data = GameRoot.Instance.GameMiniGuideDataContainer.GetData(LuaPoint.MiniGuideData_Index);
		if (data != null)
		{
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MiniGuidePopup>().Open(data.GameGuideIndex);
		}
	}
}
