using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NGuildAttendance;

public class GuildAttendanceComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate, AttendanceRewardComponent.Listener, CurrencyIconComponent.Listener, ItemIconComponent.Listener
{
	public new interface IListener
	{
		void ClickRewardList();

		void ClickMiniGuide();

		void RequestSetAttendanceReward(Action successAction, int today);

		void RequestSendPerfectAttendanceReward(Action successAction);
	}

	public struct GuildAttendanceCompInfo
	{
		public IListener IListener;

		public bool IsPopup;
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildAttendanceComponent";

	public static PrefabAuto<GuildAttendanceComponent> PrefabAuto = new PrefabAuto<GuildAttendanceComponent>("Prefab2/Component/Guild/GuildAttendanceComponent");

	public Transform RootTransform;

	public Transform BGTransform;

	public UIGrid DailyRewardComponentGrid;

	public UIGrid DailyCollaborationRewardComponentGrid;

	public UIGrid WeeklyCollaborationRewardComponentGrid;

	public Transform PerfectAttendanceRewardIconRootTransform;

	public Transform PerfectAttendanceRewardEffectTransform;

	public UILabel PerfectAttendanceRewardNameLabel;

	public UILabel PerfectAttendanceCountLabel;

	public UILabel PerfectAttendanceRewardDayLabel;

	public UIProgressBar DailyCollaborationProgress;

	public UIProgressBar WeeklyCollaborationProgress;

	public Transform InformNoGuildMemberTranform;

	public float AttendancePositionX;

	private List<AttendanceRewardComponent> _dailyRewardComponentList = new List<AttendanceRewardComponent>();

	private List<AttendanceRewardComponent> _dailyCollaborationRewardComponentList = new List<AttendanceRewardComponent>();

	private List<AttendanceRewardComponent> _weeklyCollaborationRewardComponentList = new List<AttendanceRewardComponent>();

	private GuildAttendanceCompInfo? _info;

	private int _perfectAttendanceRewarableCount;

	private bool _isGetTodayReward;

	private int _today;

	private AttendanceRewardComponent _todayDailyAttendanceRewardComponent;

	private int PerfectAttendanceCount
	{
		get
		{
			if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || !GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild)
			{
				return 0;
			}
			return GlobalAutoBehaviour<GuildManager>.instance.GuildMemberAttendanceInfo?.SuccessiveCount ?? 0;
		}
	}

	private bool CheckPerfectAttendanceGetLastStepReward
	{
		get
		{
			if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
			{
				return false;
			}
			GuildAttendanceRewardData dataPerfectAttendanceLastStep = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetDataPerfectAttendanceLastStep();
			if (dataPerfectAttendanceLastStep == null)
			{
				return false;
			}
			GuildMemberAttendanceInfo guildMemberAttendanceInfo = GlobalAutoBehaviour<GuildManager>.instance.GuildMemberAttendanceInfo;
			if (guildMemberAttendanceInfo == null)
			{
				return false;
			}
			if (dataPerfectAttendanceLastStep.Step <= guildMemberAttendanceInfo.Successive)
			{
				return false;
			}
			return true;
		}
	}

	private bool CheckPerfectAttendanceRewardable
	{
		get
		{
			if (!CheckPerfectAttendanceGetLastStepReward)
			{
				return false;
			}
			return _perfectAttendanceRewarableCount <= PerfectAttendanceCount;
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void Open(GuildAttendanceCompInfo info)
	{
		Init(info);
	}

	public void Close()
	{
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Init(GuildAttendanceCompInfo info)
	{
		_info = info;
		UIHelper.EnableGameObject(BGTransform, !_info.Value.IsPopup);
		SetPerfectAttendanceInfo();
		SetDailyRewardComponentList();
		SetDailyCollaborationInfo();
		SetWeeklyCollaborationInfo();
		InformNoGuildMember();
	}

	private void Refresh()
	{
		SetCollaborationProgress(EGuildAttendanceType.DailyCollaboration);
		SetCollaborationGetReward(EGuildAttendanceType.DailyCollaboration, ref _dailyCollaborationRewardComponentList);
		SetCollaborationProgress(EGuildAttendanceType.WeeklyCollaboration);
		SetCollaborationGetReward(EGuildAttendanceType.WeeklyCollaboration, ref _weeklyCollaborationRewardComponentList);
		SetPerfectAttendanceInfo();
	}

	private void ClearDailyRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_dailyRewardComponentList);
	}

	private void ClearDailyCollaborationRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_dailyCollaborationRewardComponentList);
	}

	private void ClearWeeklyCollaborationRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_weeklyCollaborationRewardComponentList);
	}

	private void InformNoGuildMember()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			UIHelper.EnableGameObject(InformNoGuildMemberTranform, !GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild);
		}
	}

	private void SetPerfectAttendanceInfo()
	{
		SetPerfectAttendanceCountLabel();
		SetPerfectAttendanceRewardComponent();
		UIHelper.EnableGameObject(PerfectAttendanceRewardEffectTransform, CheckPerfectAttendanceRewardable);
	}

	private void SetPerfectAttendanceCountLabel()
	{
		UIHelper.SetLabelColor(PerfectAttendanceCountLabel, Color.white);
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_DAY_COUNT", "[ff3c3c]" + PerfectAttendanceCount + "[-]");
		UIHelper.SetLabelText(PerfectAttendanceCountLabel, stringDirect);
	}

	private void SetPerfectAttendanceRewardComponent()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			int step = 0;
			GuildMemberAttendanceInfo guildMemberAttendanceInfo = GlobalAutoBehaviour<GuildManager>.instance.GuildMemberAttendanceInfo;
			bool checkIsGuild = GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild;
			if (guildMemberAttendanceInfo != null && CheckPerfectAttendanceGetLastStepReward && checkIsGuild)
			{
				step = guildMemberAttendanceInfo.Successive;
			}
			GuildAttendanceRewardData perfectAttendanceRewardDataByStep = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetPerfectAttendanceRewardDataByStep(step);
			if (perfectAttendanceRewardDataByStep != null)
			{
				_perfectAttendanceRewarableCount = perfectAttendanceRewardDataByStep.Day;
				UIHelper.SetLabelText(PerfectAttendanceRewardDayLabel, LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_PERFECT_REWARD_DAY", perfectAttendanceRewardDataByStep.Day));
				FactoryBase.ReleaseComponent(PerfectAttendanceRewardIconRootTransform.GetComponentInChildren(typeof(ItemIconComponent)));
				FactoryBase.ReleaseComponent(PerfectAttendanceRewardIconRootTransform.GetComponentInChildren(typeof(CurrencyIconComponent)));
				Component component = null;
				int rewardCount = 0;
				SetRewardIconComponent(ref PerfectAttendanceRewardIconRootTransform, perfectAttendanceRewardDataByStep, out component, out rewardCount, PerfectAttendanceRewardNameLabel);
			}
		}
	}

	private void SetDailyCollaborationInfo()
	{
		SetDailyCollaborationRewardComponentList();
		SetCollaborationProgress(EGuildAttendanceType.DailyCollaboration);
	}

	private void SetDailyCollaborationRewardComponentList()
	{
		ClearDailyCollaborationRewardComponentList();
		if (DailyCollaborationRewardComponentGrid == null)
		{
			return;
		}
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(EGuildAttendanceType.DailyCollaboration);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		int collaborationAttendanceCount = GetCollaborationAttendanceCount(EGuildAttendanceType.DailyCollaboration);
		AttendanceRewardComponent attendanceRewardComponent = null;
		GuildAttendanceRewardData guildAttendanceRewardData = null;
		Component component = null;
		int rewardCount = 0;
		for (int i = 0; i < list.Count; i++)
		{
			guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData != null)
			{
				attendanceRewardComponent = AttendanceRewardComponent.PrefabGuildCollaborationAuto.Factory.Create(DailyCollaborationRewardComponentGrid.transform);
				if (!(attendanceRewardComponent == null))
				{
					SetRewardIconComponent(ref attendanceRewardComponent.ItemIconRoot, guildAttendanceRewardData, out component, out rewardCount);
					SetIconComponent(ref component, isToday: false, rewardCount, isGrayScale: false);
					attendanceRewardComponent.SetDay(guildAttendanceRewardData.Day);
					attendanceRewardComponent.SetAlreadyRewarded(collaborationAttendanceCount >= guildAttendanceRewardData.Day);
					_dailyCollaborationRewardComponentList.Add(attendanceRewardComponent);
				}
			}
		}
		UIHelper.ResetGridPosition(DailyCollaborationRewardComponentGrid);
	}

	private void SetWeeklyCollaborationInfo()
	{
		SetWeeklyCollaborationRewardComponentList();
		SetCollaborationProgress(EGuildAttendanceType.WeeklyCollaboration);
	}

	private void SetWeeklyCollaborationRewardComponentList()
	{
		ClearWeeklyCollaborationRewardComponentList();
		if (WeeklyCollaborationRewardComponentGrid == null)
		{
			return;
		}
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(EGuildAttendanceType.WeeklyCollaboration);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		int collaborationAttendanceCount = GetCollaborationAttendanceCount(EGuildAttendanceType.WeeklyCollaboration);
		AttendanceRewardComponent attendanceRewardComponent = null;
		GuildAttendanceRewardData guildAttendanceRewardData = null;
		Component component = null;
		int rewardCount = 0;
		for (int i = 0; i < list.Count; i++)
		{
			guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData != null)
			{
				attendanceRewardComponent = AttendanceRewardComponent.PrefabGuildCollaborationAuto.Factory.Create(WeeklyCollaborationRewardComponentGrid.transform);
				if (!(attendanceRewardComponent == null))
				{
					SetRewardIconComponent(ref attendanceRewardComponent.ItemIconRoot, guildAttendanceRewardData, out component, out rewardCount);
					SetIconComponent(ref component, isToday: false, rewardCount, isGrayScale: false);
					attendanceRewardComponent.SetDay(guildAttendanceRewardData.Day);
					attendanceRewardComponent.SetAlreadyRewarded(collaborationAttendanceCount >= guildAttendanceRewardData.Day);
					_weeklyCollaborationRewardComponentList.Add(attendanceRewardComponent);
				}
			}
		}
		UIHelper.ResetGridPosition(WeeklyCollaborationRewardComponentGrid);
	}

	private void SetDailyRewardComponentList()
	{
		ClearDailyRewardComponentList();
		if (DailyRewardComponentGrid == null || !GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return;
		}
		List<int> guildAttendanceDayList = GlobalAutoBehaviour<GuildManager>.instance.GuildAttendanceDayList;
		int guildAttendanceToday = GlobalAutoBehaviour<GuildManager>.instance.GuildAttendanceToday;
		bool checkIsGuild = GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild;
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(EGuildAttendanceType.Daily);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		AttendanceRewardComponent attendanceRewardComponent = null;
		GuildAttendanceRewardData guildAttendanceRewardData = null;
		Component component = null;
		bool flag = false;
		int rewardCount = 0;
		for (int i = 0; i < list.Count; i++)
		{
			guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData == null)
			{
				continue;
			}
			attendanceRewardComponent = AttendanceRewardComponent.PrefabGuildDailyAuto.Factory.Create(DailyRewardComponentGrid.transform);
			if (!(attendanceRewardComponent == null))
			{
				flag = false;
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(guildAttendanceDayList) && guildAttendanceDayList.Count > i && guildAttendanceDayList[i] == 1 && checkIsGuild)
				{
					flag = true;
				}
				bool flag2 = i < guildAttendanceToday && !flag;
				SetRewardIconComponent(ref attendanceRewardComponent.ItemIconRoot, guildAttendanceRewardData, out component, out rewardCount);
				SetIconComponent(ref component, guildAttendanceToday == i, rewardCount, flag2);
				attendanceRewardComponent.SetDayLabel(LocalizationManager.GetStringDirect("GUILD_ATTENDANCE_DAY_CHECK", guildAttendanceRewardData.Day));
				attendanceRewardComponent.SetListener((guildAttendanceToday == i) ? this : null);
				attendanceRewardComponent.SetAlreadyRewarded(flag);
				attendanceRewardComponent.SetTodayReward(!flag && guildAttendanceToday == i);
				attendanceRewardComponent.SetGrayScale(flag2);
				UIHelper.EnableGameObject(attendanceRewardComponent.EffectWidget, enable: false);
				_dailyRewardComponentList.Add(attendanceRewardComponent);
				if (guildAttendanceToday == i && !flag)
				{
					_isGetTodayReward = false;
					_today = guildAttendanceToday + 1;
					_todayDailyAttendanceRewardComponent = attendanceRewardComponent;
				}
			}
		}
		DailyRewardComponentGrid.Reposition();
	}

	private void SetCollaborationProgress(EGuildAttendanceType type)
	{
		float collaborationProgress = GetCollaborationProgress(type);
		float num = GetLastCollaborationCount(type) - GetFirstCollaborationCount(type);
		switch (type)
		{
		case EGuildAttendanceType.DailyCollaboration:
			DailyCollaborationProgress.value = collaborationProgress / num;
			break;
		case EGuildAttendanceType.WeeklyCollaboration:
			WeeklyCollaborationProgress.value = collaborationProgress / num;
			break;
		}
	}

	private void SetCollaborationGetReward(EGuildAttendanceType type, ref List<AttendanceRewardComponent> componentList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(componentList))
		{
			return;
		}
		int collaborationAttendanceCount = GetCollaborationAttendanceCount(type);
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(type);
		AttendanceRewardComponent attendanceRewardComponent = null;
		bool flag = false;
		for (int i = 0; i < componentList.Count; i++)
		{
			attendanceRewardComponent = componentList[i];
			if (!(attendanceRewardComponent == null))
			{
				flag = false;
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(list) && list.Count > i && list[i].Day <= collaborationAttendanceCount)
				{
					flag = true;
				}
				attendanceRewardComponent.SetAlreadyRewarded(flag);
			}
		}
	}

	private void SetRewardIconComponent(ref Transform iconRootTranform, GuildAttendanceRewardData data, out Component component, out int rewardCount, UILabel rewardNameLabel = null)
	{
		component = null;
		rewardCount = 0;
		if (iconRootTranform == null)
		{
			return;
		}
		FactoryBase.ReleaseComponent(iconRootTranform.GetComponentInChildren(typeof(ItemIconComponent)));
		FactoryBase.ReleaseComponent(iconRootTranform.GetComponentInChildren(typeof(CurrencyIconComponent)));
		if (data == null)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		if (data2 == null)
		{
			return;
		}
		List<RewardElementData> elementList = data2.GetElementList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(elementList))
		{
			return;
		}
		string rewardName = string.Empty;
		RewardElementData rewardElementData = null;
		for (int i = 0; i < elementList.Count; i++)
		{
			rewardElementData = elementList[i];
			component = CreateRewardIconComponent(elementList[i], iconRootTranform, out rewardName);
			if (rewardElementData != null)
			{
				rewardCount = rewardElementData.ItemMin;
			}
		}
		UIHelper.SetLabelText(rewardNameLabel, rewardName + " x" + rewardCount);
	}

	private void SetIconComponent(ref Component component, bool isToday, int rewardCount, bool isGrayScale)
	{
		if (!(component == null))
		{
			ItemIconComponent itemIconComponent = component as ItemIconComponent;
			if (itemIconComponent != null)
			{
				itemIconComponent.SetListener(isToday ? this : null);
				itemIconComponent.SetIconGrayScale(isGrayScale);
				itemIconComponent.SetItemCount(rewardCount);
			}
			CurrencyIconComponent currencyIconComponent = component as CurrencyIconComponent;
			if (currencyIconComponent != null)
			{
				currencyIconComponent.SetListener(isToday ? this : null);
				currencyIconComponent.SetIconGrayScale(isGrayScale);
			}
		}
	}

	private void RequestSetDailyAttendanceReward()
	{
		if (!_isGetTodayReward && !(_todayDailyAttendanceRewardComponent == null) && _info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.RequestSetAttendanceReward(delegate
			{
				_isGetTodayReward = true;
				_todayDailyAttendanceRewardComponent.SetTodayReward(today: false);
				UIHelper.EnableGameObject(_todayDailyAttendanceRewardComponent.EffectWidget, enable: true);
				_todayDailyAttendanceRewardComponent.StartReceiveEffect();
				Refresh();
			}, _today);
		}
	}

	private Component CreateRewardIconComponent(RewardElementData data, Transform rootTransform, out string rewardName)
	{
		rewardName = string.Empty;
		if (data == null)
		{
			return null;
		}
		if (rootTransform == null || rootTransform.childCount > 0)
		{
			return null;
		}
		Component component = null;
		if (data.IsGold() || data.IsGem())
		{
			component = CurrencyIconComponent.PrefabAuto.Factory.Create(rootTransform);
			if (component == null)
			{
				return null;
			}
			CurrencyIconComponent obj = component as CurrencyIconComponent;
			CurrencyType currencyType = data.GetCurrencyType();
			int itemMin = data.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			obj.Open(currencyType, num, listener);
		}
		else
		{
			component = ItemIconComponent.PrefabAuto.Factory.Create(rootTransform);
			if (component == null)
			{
				return null;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode);
			if (dataByCode == null)
			{
				return null;
			}
			(component as ItemIconComponent).Open(dataByCode);
			rewardName = dataByCode.Name;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(component.transform);
		component.transform.localScale = rootTransform.localScale;
		return component;
	}

	private float GetCollaborationProgress(EGuildAttendanceType type)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return 0f;
		}
		int collaborationAttendanceCount = GetCollaborationAttendanceCount(type);
		float firstCollaborationCount = GetFirstCollaborationCount(type);
		if (!((float)collaborationAttendanceCount > firstCollaborationCount))
		{
			return 0f;
		}
		return (float)collaborationAttendanceCount - firstCollaborationCount;
	}

	private float GetFirstCollaborationCount(EGuildAttendanceType type)
	{
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(type);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return 0f;
		}
		list.Sort((GuildAttendanceRewardData data, GuildAttendanceRewardData compData) => compData.Day.CompareTo(data.Day));
		return list[list.Count - 1].Day;
	}

	private float GetLastCollaborationCount(EGuildAttendanceType type)
	{
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(type);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return 0f;
		}
		list.Sort((GuildAttendanceRewardData data, GuildAttendanceRewardData compData) => data.Day.CompareTo(compData.Day));
		return list[list.Count - 1].Day;
	}

	private int GetCollaborationAttendanceCount(EGuildAttendanceType type)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance || !GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild)
		{
			return 0;
		}
		return type switch
		{
			EGuildAttendanceType.DailyCollaboration => GlobalAutoBehaviour<GuildManager>.instance.DailyCollaborationCout, 
			EGuildAttendanceType.WeeklyCollaboration => GlobalAutoBehaviour<GuildManager>.instance.WeeklyCollaborationCout, 
			_ => 0, 
		};
	}

	public void OnClickRewardList()
	{
		if (_info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.ClickRewardList();
		}
	}

	public void OnClickMiniGuide()
	{
		if (_info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.ClickMiniGuide();
		}
	}

	public void OnClickPerfectAttendanceReward()
	{
		if (CheckPerfectAttendanceRewardable && _info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.RequestSendPerfectAttendanceReward(delegate
			{
				Refresh();
			});
		}
	}

	void AttendanceRewardComponent.Listener.OnClick(AttendanceRewardComponent component)
	{
		if (!(component == null))
		{
			RequestSetDailyAttendanceReward();
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent currencyIconComponent)
	{
		if (!(currencyIconComponent == null))
		{
			RequestSetDailyAttendanceReward();
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null))
		{
			RequestSetDailyAttendanceReward();
		}
	}
}
