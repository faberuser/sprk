using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SoulWeaponAbilityTicketConfirmPopup : BaseWindow
{
	public enum AbilityState
	{
		AbilityConfirm,
		AbilityEffectPlaying,
		AbilityComplete
	}

	public class SoulWeaponStatListInfo
	{
		public int Grade;

		public int ReinforceLevel;

		public int CurrentExp;

		public int RequiredExp;

		public float SuccessRate;

		public float SoulWeaponAtk;

		public float SoulWeaponHp;

		public bool CanUseSkill;

		public SoulWeaponStatListInfo(SoulWeaponInfo info)
		{
			if (info == null)
			{
				CanUseSkill = false;
				Grade = -1;
				ReinforceLevel = -1;
				CurrentExp = -1;
				RequiredExp = -1;
				SuccessRate = -1f;
				SoulWeaponAtk = -1f;
				SoulWeaponHp = -1f;
				return;
			}
			CanUseSkill = true;
			Grade = info.Grade;
			ReinforceLevel = info.ReinforceLevel;
			CurrentExp = info.Exp;
			SoulWeaponReinforceData soulWeaponReinforceData = info.GetSoulWeaponReinforceData();
			if (soulWeaponReinforceData != null)
			{
				RequiredExp = soulWeaponReinforceData.RequiredExp;
			}
			SuccessRate = (float)System.Math.Ceiling((float)info.ReinforceRatio / 10f) / 100f;
			SoulWeaponAtk = info.GetBaseStat(ESoulWeaponStatType.Attack, isRenew: false);
			SoulWeaponHp = info.GetBaseStat(ESoulWeaponStatType.Hp, isRenew: false);
		}

		public bool IsSoulWeapon()
		{
			return CanUseSkill;
		}
	}

	public interface IListener
	{
		void OnClickConfirmPopup(int itemIndex, int targetEquipItemSlotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/SoulWeaponAbilityTicketConfirmPopup";

	public UILabel Label_Title;

	public UILabel Label_Desc;

	public EquipTransitionTicket.TransitionItemInfoAndComponent AbilityItemInfo;

	public EquipTransitionTicket.TransitionItemInfoAndComponent AfterAbilityInfo;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	public UILabel Label_UseTicket;

	public UIGame3DWidget SoulWeaponAbilityUseEffect;

	public float TicketUseResultSetTime = 1f;

	public UIButton Button_Close;

	public UIWidget DragScrollView;

	public UIButton Button_UseTicket;

	[Header("[Color Template]")]
	public Color StatFieldColor = new Color32(231, 214, 196, byte.MaxValue);

	public Color StatBeforeValueColor = new Color32(168, 159, 142, byte.MaxValue);

	public Color StatAfterPlusColor = new Color32(2, byte.MaxValue, 218, byte.MaxValue);

	public Color StatAfterMinusColor = Color.red;

	private IListener _listener;

	public SoulWeaponAbilityTicket.SoulWeaponAbilityItemDataInfo AbilityItemDataInfo;

	public SoulWeaponStatListInfo BeforeSoulWeaponStatInfo;

	public SoulWeaponStatListInfo AfterSoulWeaponStatInfo;

	private StatListComponent _statListComponent;

	private SoulWeaponAbilityTicketData _soulWeaponAbilityTicketData;

	public AbilityState CurrentState { get; private set; }

	public StatListComponent.ColorInfos ColorInfos { get; private set; }

	public void Open(IListener listener, SoulWeaponAbilityTicket.SoulWeaponAbilityItemDataInfo abilityItemDataInfo, SoulWeaponAbilityTicketData soulWeaponAbilityTicketData)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
			return;
		}
		_listener = listener;
		CurrentState = AbilityState.AbilityConfirm;
		AbilityItemDataInfo = abilityItemDataInfo;
		_soulWeaponAbilityTicketData = soulWeaponAbilityTicketData;
		base.Open();
		Init();
	}

	private void Init()
	{
		InitColorInfos();
		InitSoulWeaponStatListInfo();
		SetLabelTexts();
		SetEquipItemIcon(AbilityItemDataInfo.EquipItemInfo);
		SetupSoulWeaponStatInfo();
	}

	private void InitColorInfos()
	{
		ColorInfos = new StatListComponent.ColorInfos
		{
			StatColor = new StatListComponent.ColorInfo(StatFieldColor),
			BeforeValueColor = new StatListComponent.ColorInfo(StatBeforeValueColor),
			AfterPlusValueColor = new StatListComponent.ColorInfo(StatAfterPlusColor),
			AfterMinusValueColor = new StatListComponent.ColorInfo(StatAfterMinusColor)
		};
	}

	private void SetLabelTexts()
	{
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(AbilityItemDataInfo.UseItemData.Name));
		if (CurrentState == AbilityState.AbilityConfirm)
		{
			UIHelper.SetLabelText(Label_Desc, LocalizationManager.GetStringDirect("SOULWEAPON_ABILITY_TICKET_INFO_DESC"));
			UIHelper.SetLabelText(Label_UseTicket, LocalizationManager.GetStringDirect("INVENTORY_USE_ITEM"));
		}
		else
		{
			UIHelper.SetLabelText(Label_Desc, LocalizationManager.GetStringDirect("SOULWEAPON_ABILITY_TICKET_COMPLETE"));
			UIHelper.SetLabelText(Label_UseTicket, LocalizationManager.GetStringDirect("COMMON_CONFIRM_01"));
		}
	}

	private void SetEquipItemIcon(EquipItemInfo equipItemInfo)
	{
		UIHelper.EnableGameObject(AbilityItemInfo.ItemInfoComponent, enable: true);
		UIHelper.EnableGameObject(AfterAbilityInfo.ItemInfoComponent, enable: false);
		AbilityItemInfo.ClearEquipItemIcons();
		AbilityItemInfo.SetGiveEquipItemIcon(equipItemInfo);
		EquipItemInfo afterEquipItemInfo = SoulWeaponAbilityTicket.GetAfterEquipItemInfo(equipItemInfo, _soulWeaponAbilityTicketData);
		AbilityItemInfo.SetGetEquipItemIcon(afterEquipItemInfo);
	}

	private void SetAfterEquipItemIcon(EquipItemInfo equipItemInfo)
	{
		UIHelper.EnableGameObject(AbilityItemInfo.ItemInfoComponent, enable: false);
		UIHelper.EnableGameObject(AfterAbilityInfo.ItemInfoComponent, enable: true);
		AbilityItemInfo.ClearEquipItemIcons();
		AfterAbilityInfo.ClearEquipItemIcons();
		AfterAbilityInfo.SetGiveEquipItemIcon(equipItemInfo);
	}

	private void InitSoulWeaponStatListInfo()
	{
		BeforeSoulWeaponStatInfo = new SoulWeaponStatListInfo(AbilityItemDataInfo.EquipItemInfo.SoulWeaponInfo);
		EquipItemInfo afterEquipItemInfo = SoulWeaponAbilityTicket.GetAfterEquipItemInfo(AbilityItemDataInfo.EquipItemInfo, _soulWeaponAbilityTicketData);
		AfterSoulWeaponStatInfo = new SoulWeaponStatListInfo(afterEquipItemInfo.SoulWeaponInfo);
	}

	private void SetupSoulWeaponStatInfo()
	{
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		_ = BeforeSoulWeaponStatInfo;
		_ = AfterSoulWeaponStatInfo;
		switch (_soulWeaponAbilityTicketData.ItemType)
		{
		case ESoulWeaponAbilityTicketType.Grade:
			SetupSoulWeaponGradeStatInfo();
			break;
		case ESoulWeaponAbilityTicketType.Ether:
			SetupSoulWeaponEtherStatInfo();
			break;
		case ESoulWeaponAbilityTicketType.Jumping:
			SetupSoulWeaponJumpingStatInfo();
			break;
		}
	}

	private void SetupSoulWeaponGradeStatInfo()
	{
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		SoulWeaponStatListInfo beforeStatInfo = BeforeSoulWeaponStatInfo;
		SoulWeaponStatListInfo afterStatInfo = AfterSoulWeaponStatInfo;
		string stringDirect = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_UPGRADE");
		_statListComponent.AddIntegerStatChange(stringDirect, beforeStatInfo.Grade, afterStatInfo.Grade, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect2 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ATTACK");
		_statListComponent.AddFloatStatChange(stringDirect2, beforeStatInfo.SoulWeaponAtk, afterStatInfo.SoulWeaponAtk, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect3 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_HP");
		_statListComponent.AddFloatStatChange(stringDirect3, beforeStatInfo.SoulWeaponHp, afterStatInfo.SoulWeaponHp, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect4 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_SKILL");
		string stringDirect5 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_POSSIBLE");
		string stringDirect6 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_IMPOSSIBLE");
		string before = (beforeStatInfo.CanUseSkill ? stringDirect5 : stringDirect6);
		string after = (afterStatInfo.CanUseSkill ? stringDirect5 : stringDirect6);
		_statListComponent.AddCustomStringFormatStatChange(stringDirect4, before, after, () => afterStatInfo.CanUseSkill.CompareTo(beforeStatInfo.CanUseSkill), StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
	}

	private void SetupSoulWeaponEtherStatInfo()
	{
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		SoulWeaponStatListInfo beforeStatInfo = BeforeSoulWeaponStatInfo;
		SoulWeaponStatListInfo afterStatInfo = AfterSoulWeaponStatInfo;
		bool flag = IsMaxReinforceLevel(beforeStatInfo.ReinforceLevel);
		bool isAfterStatMaxReinforceLevel = IsMaxReinforceLevel(afterStatInfo.ReinforceLevel);
		string stringDirect = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_REINFORCE");
		_statListComponent.AddIntegerStatChange(stringDirect, beforeStatInfo.ReinforceLevel, afterStatInfo.ReinforceLevel, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect2 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_FIGURE");
		string text = "";
		text = ((!beforeStatInfo.IsSoulWeapon() || flag) ? "-/-" : $"{beforeStatInfo.CurrentExp}/{beforeStatInfo.RequiredExp}");
		string text2 = "";
		text2 = ((!afterStatInfo.IsSoulWeapon() || isAfterStatMaxReinforceLevel) ? "-/-" : $"{afterStatInfo.CurrentExp}/{afterStatInfo.RequiredExp}");
		_statListComponent.AddCustomStringFormatStatChange(stringDirect2, text, text2, () => 0, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		string stringDirect3 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_REINFORCE_SUCCESS");
		float beforeSuccessRate = ((beforeStatInfo.SuccessRate > 100f) ? 100f : beforeStatInfo.SuccessRate);
		string before = ((!flag && beforeStatInfo.IsSoulWeapon()) ? $"{beforeSuccessRate}%" : "-%");
		float afterSuccessRate = ((afterStatInfo.SuccessRate > 100f) ? 100f : afterStatInfo.SuccessRate);
		string after = ((!isAfterStatMaxReinforceLevel && afterStatInfo.IsSoulWeapon()) ? $"{afterSuccessRate}%" : "-%");
		_statListComponent.AddCustomStringFormatStatChange(stringDirect3, before, after, delegate
		{
			if (isAfterStatMaxReinforceLevel)
			{
				return 0;
			}
			if (!beforeStatInfo.IsSoulWeapon())
			{
				return 0;
			}
			return afterStatInfo.IsSoulWeapon() ? afterSuccessRate.CompareTo(beforeSuccessRate) : 0;
		}, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		string stringDirect4 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ATTACK");
		_statListComponent.AddFloatStatChange(stringDirect4, beforeStatInfo.SoulWeaponAtk, afterStatInfo.SoulWeaponAtk, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect5 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_HP");
		_statListComponent.AddFloatStatChange(stringDirect5, beforeStatInfo.SoulWeaponHp, afterStatInfo.SoulWeaponHp, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
	}

	private void SetupSoulWeaponJumpingStatInfo()
	{
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		SoulWeaponStatListInfo beforeStatInfo = BeforeSoulWeaponStatInfo;
		SoulWeaponStatListInfo afterStatInfo = AfterSoulWeaponStatInfo;
		bool flag = IsMaxReinforceLevel(beforeStatInfo.ReinforceLevel);
		bool isAfterStatMaxReinforceLevel = IsMaxReinforceLevel(afterStatInfo.ReinforceLevel);
		string stringDirect = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_UPGRADE");
		_statListComponent.AddIntegerStatChange(stringDirect, beforeStatInfo.Grade, afterStatInfo.Grade, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect2 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_REINFORCE");
		_statListComponent.AddIntegerStatChange(stringDirect2, beforeStatInfo.ReinforceLevel, afterStatInfo.ReinforceLevel, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect3 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_FIGURE");
		string text = "";
		text = ((!beforeStatInfo.IsSoulWeapon() || flag) ? "-/-" : $"{beforeStatInfo.CurrentExp}/{beforeStatInfo.RequiredExp}");
		string text2 = "";
		text2 = ((!afterStatInfo.IsSoulWeapon() || isAfterStatMaxReinforceLevel) ? "-/-" : $"{afterStatInfo.CurrentExp}/{afterStatInfo.RequiredExp}");
		_statListComponent.AddCustomStringFormatStatChange(stringDirect3, text, text2, () => 0, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		string stringDirect4 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_REINFORCE_SUCCESS");
		float beforeSuccessRate = ((beforeStatInfo.SuccessRate > 100f) ? 100f : beforeStatInfo.SuccessRate);
		string before = ((!flag && beforeStatInfo.IsSoulWeapon()) ? $"{beforeSuccessRate}%" : "-%");
		float afterSuccessRate = ((afterStatInfo.SuccessRate > 100f) ? 100f : afterStatInfo.SuccessRate);
		string after = ((!isAfterStatMaxReinforceLevel && afterStatInfo.IsSoulWeapon()) ? $"{afterSuccessRate}%" : "-%");
		_statListComponent.AddCustomStringFormatStatChange(stringDirect4, before, after, delegate
		{
			if (isAfterStatMaxReinforceLevel)
			{
				return 0;
			}
			if (!beforeStatInfo.IsSoulWeapon())
			{
				return 0;
			}
			return afterStatInfo.IsSoulWeapon() ? afterSuccessRate.CompareTo(beforeSuccessRate) : 0;
		}, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		string stringDirect5 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ATTACK");
		_statListComponent.AddFloatStatChange(stringDirect5, beforeStatInfo.SoulWeaponAtk, afterStatInfo.SoulWeaponAtk, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect6 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_HP");
		_statListComponent.AddFloatStatChange(stringDirect6, beforeStatInfo.SoulWeaponHp, afterStatInfo.SoulWeaponHp, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect7 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_SKILL");
		string stringDirect8 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_POSSIBLE");
		string stringDirect9 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_IMPOSSIBLE");
		string before2 = (beforeStatInfo.CanUseSkill ? stringDirect8 : stringDirect9);
		string after2 = (afterStatInfo.CanUseSkill ? stringDirect8 : stringDirect9);
		_statListComponent.AddCustomStringFormatStatChange(stringDirect7, before2, after2, () => afterStatInfo.CanUseSkill.CompareTo(beforeStatInfo.CanUseSkill), StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
	}

	public static bool IsMaxReinforceLevel(int reinforceLevel)
	{
		SoulWeaponReinforceData data = GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(reinforceLevel + 1);
		if (data == null)
		{
			return true;
		}
		return reinforceLevel == data.ReinforceLevel;
	}

	public void SetUseItemComplete(int completeSlotIndex)
	{
		SetLabelTexts();
		EquipItemInfo equipItemBySlotIndex = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(completeSlotIndex);
		SetAfterEquipItemIcon(equipItemBySlotIndex);
		SetResultStatInfo(equipItemBySlotIndex);
	}

	private void SetResultStatInfo(EquipItemInfo equipItemInfo)
	{
		AfterSoulWeaponStatInfo = new SoulWeaponStatListInfo(equipItemInfo.SoulWeaponInfo);
		SetupSoulWeaponStatInfo();
	}

	public override void Close()
	{
		if (CurrentState != AbilityState.AbilityEffectPlaying)
		{
			AbilityItemInfo.ClearEquipItemIcons();
			AfterAbilityInfo.ClearEquipItemIcons();
			ClearStatChange();
			base.Close();
		}
	}

	private void ClearStatChange()
	{
		if (!(_statListComponent == null))
		{
			FactoryBase.ReleaseComponent(_statListComponent);
			_statListComponent = null;
		}
	}

	public void SetSoulWeaponAbilityEffectPlay(Action setUseAbilityItemResult, Action openRewardPopup)
	{
		if (!(SoulWeaponAbilityUseEffect == null) && !SoulWeaponAbilityUseEffect.IsPlaying)
		{
			VespaCoroutine.Start(coSoulWeaponAbilityEffectPlay(setUseAbilityItemResult, openRewardPopup));
		}
	}

	private IEnumerator coSoulWeaponAbilityEffectPlay(Action setAbilityResult, Action openRewardPopup)
	{
		CurrentState = AbilityState.AbilityEffectPlaying;
		SoulWeaponAbilityUseEffect.Play(delegate
		{
			CurrentState = AbilityState.AbilityComplete;
			SetColliderEnable(isEnable: true);
			Vespa_System_Action_Extension_Method.fire(openRewardPopup);
		});
		yield return new WaitForSeconds(TicketUseResultSetTime);
		Vespa_System_Action_Extension_Method.fire(setAbilityResult);
	}

	public void SetColliderEnable(bool isEnable)
	{
		UIHelper.EnableCollider(Button_Close, isEnable);
		UIHelper.EnableCollider(DragScrollView, isEnable);
		UIHelper.EnableCollider(Button_UseTicket, isEnable);
	}

	public void OnClickUse()
	{
		if (_listener == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("OPTION_TAP_PUSH");
		string message = string.Empty;
		switch (_soulWeaponAbilityTicketData.ItemType)
		{
		case ESoulWeaponAbilityTicketType.Grade:
			message = LocalizationManager.GetStringDirect("SOULWEAPON_UPGRADE_TICKET_CONFIRM_DESC");
			break;
		case ESoulWeaponAbilityTicketType.Ether:
			message = LocalizationManager.GetStringDirect("SOULWEAPON_REINFORCE_TICKET_CONFIRM_DESC");
			break;
		case ESoulWeaponAbilityTicketType.Jumping:
			message = LocalizationManager.GetStringDirect("SOULWEAPON_JUMPING_TICKET_CONFIRM_DESC");
			break;
		}
		switch (CurrentState)
		{
		case AbilityState.AbilityConfirm:
			MessagePopup3Button.Open(stringDirect, message, delegate
			{
				SetColliderEnable(isEnable: false);
				int index = AbilityItemDataInfo.UseItemData.Index;
				int slotIndex = AbilityItemDataInfo.EquipItemInfo.SlotIndex;
				_listener.OnClickConfirmPopup(index, slotIndex);
			}, null);
			break;
		case AbilityState.AbilityComplete:
			Close();
			break;
		}
	}
}
