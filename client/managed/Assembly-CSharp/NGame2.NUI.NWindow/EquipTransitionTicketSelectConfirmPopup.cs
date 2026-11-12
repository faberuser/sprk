using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipTransitionTicketSelectConfirmPopup : BaseWindow, ItemIconComponent.Listener
{
	public enum TransitionState
	{
		TransitionConfirm,
		TransitionEffectPlaying,
		TransitionComplete
	}

	public class AwakenStatListInfo
	{
		public int Star;

		public int FailBonus;

		public StatData ItemStat;

		public AwakenStatListInfo(EquipItemInfo equipItemInfo, int star, int failBonus)
		{
			Star = star;
			FailBonus = failBonus;
			ItemStat = StatCalculator.GetEquipItemStatData(equipItemInfo.ItemIndex, star, equipItemInfo.Level);
		}
	}

	public class SoulWeaponStatListInfo
	{
		public int Grade;

		public int ReinforceLevel;

		public int CurrentExp;

		public int RequiredExp;

		public float SuccessRate;

		public float SoulWeaponAtkRate;

		public float SoulWeaponHpRate;

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
				SoulWeaponAtkRate = -1f;
				SoulWeaponHpRate = -1f;
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
			SoulWeaponAtkRate = (float)info.GetOptionRatio(ESoulWeaponStatType.Attack) / 10f;
			SoulWeaponHpRate = (float)info.GetOptionRatio(ESoulWeaponStatType.Hp) / 10f;
		}

		public bool IsSoulWeapon()
		{
			return CanUseSkill;
		}
	}

	public interface IListener
	{
		void OnClickTransition(int itemIndex, int giveEquipItemSlotIndex, int getEquipItemSlotIndex, EquipTransitionType transitionType);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/EquipTransitionTicketSelectConfirmPopup";

	public UILabel Label_Title;

	public UILabel Label_Desc;

	public EquipTransitionTicket.TransitionItemInfoAndComponent TransItemInfo;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	public UILabel Label_Transition;

	public UIGame3DWidget TransitionEffect;

	public float TransitionResultSetTime = 1f;

	public UIButton Button_Close;

	public UIWidget DragScrollView;

	public UIButton Button_Transition;

	[Header("[Color Template]")]
	public Color StatFieldColor = new Color32(231, 214, 196, byte.MaxValue);

	public Color StatBeforeValueColor = new Color32(168, 159, 142, byte.MaxValue);

	public Color StatAfterPlusColor = new Color32(2, byte.MaxValue, 218, byte.MaxValue);

	public Color StatAfterMinusColor = Color.red;

	private IListener _listener;

	public EquipTransitionTicket.TransitionItemDataInfo TransItemDataInfo;

	public AwakenStatListInfo BeforeGiveAwakenStatInfo;

	public AwakenStatListInfo AfterGiveAwakenStatInfo;

	public AwakenStatListInfo BeforeGetAwakenStatInfo;

	public AwakenStatListInfo AfterGetAwakenStatInfo;

	public SoulWeaponStatListInfo BeforeGiveSoulWeaponStatInfo;

	public SoulWeaponStatListInfo AfterGiveSoulWeaponStatInfo;

	public SoulWeaponStatListInfo BeforeGetSoulWeaponStatInfo;

	public SoulWeaponStatListInfo AfterGetSoulWeaponStatInfo;

	private StatListComponent _statListComponent;

	private EquipTransitionType _transitionType;

	public TransitionState CurrentState { get; private set; }

	public bool SelectedGetItem { get; private set; }

	public StatListComponent.ColorInfos ColorInfos { get; private set; }

	public void Open(IListener listener, EquipTransitionTicket.TransitionItemDataInfo transitionItemDataInfo, EquipTransitionType transitionType)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
			return;
		}
		_listener = listener;
		CurrentState = TransitionState.TransitionConfirm;
		TransItemDataInfo = transitionItemDataInfo;
		_transitionType = transitionType;
		base.Open();
		Init();
	}

	private void Init()
	{
		InitColorInfos();
		InitStatListInfo();
		SetLabelTexts();
		SetEquipItemIcon(TransItemDataInfo.GiveEquipItemInfo, TransItemDataInfo.GetEquipItemInfo);
		SetupTransitionInfo(TransItemDataInfo.GetEquipItemInfo);
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
		bool flag = _transitionType == EquipTransitionType.Awaken;
		if (CurrentState == TransitionState.TransitionConfirm)
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(flag ? "TRANSITION_AWAKEN_CONFIRM_POPUP_TITLE" : "TRANSITION_SOULWEAPON_CONFIRM_TITLE"));
			UIHelper.SetLabelText(Label_Desc, LocalizationManager.GetStringDirect(flag ? "TRANSITION_AWAKEN_CONFIRM_POPUP_DESC" : "TRANSITION_SOULWEAPON_CONFIRM_DESC"));
			UIHelper.SetLabelText(Label_Transition, LocalizationManager.GetStringDirect(flag ? "TRANSITION_AWAKEN_BUTTON" : "TRANSITION_BUTTON"));
		}
		else
		{
			UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(flag ? "TRANSITION_AWAKEN_RESULT_POPUP_TITLE" : "TRANSITION_SOULWEAPON_RESULT_TITLE"));
			UIHelper.SetLabelText(Label_Desc, LocalizationManager.GetStringDirect(flag ? "TRANSITION_AWAKEN_RESULT_POPUP_DESC" : "TRANSITION_SOULWEAPON_RESULT_DESC"));
			UIHelper.SetLabelText(Label_Transition, LocalizationManager.GetStringDirect("COMMON_CONFIRM"));
		}
	}

	private void SetEquipItemIcon(EquipItemInfo giveEquipItemInfo, EquipItemInfo getEquipItemInfo)
	{
		TransItemInfo.ClearEquipItemIcons();
		TransItemInfo.SetGiveEquipItemIcon(giveEquipItemInfo, this);
		TransItemInfo.SetGetEquipItemIcon(getEquipItemInfo, this);
	}

	private void InitStatListInfo()
	{
		if (_transitionType == EquipTransitionType.Awaken)
		{
			InitAwakenStatListInfo();
		}
		else if (_transitionType == EquipTransitionType.SoulWeapon)
		{
			InitSoulWeaponStatListInfo();
		}
	}

	private void InitAwakenStatListInfo()
	{
		int star = TransItemDataInfo.GiveEquipItemInfo.Star;
		int upgradeStarFailBonus = TransItemDataInfo.GiveEquipItemInfo.UpgradeStarFailBonus;
		int star2 = TransItemDataInfo.GetEquipItemInfo.Star;
		int upgradeStarFailBonus2 = TransItemDataInfo.GetEquipItemInfo.UpgradeStarFailBonus;
		BeforeGiveAwakenStatInfo = new AwakenStatListInfo(TransItemDataInfo.GiveEquipItemInfo, star, upgradeStarFailBonus);
		AfterGiveAwakenStatInfo = new AwakenStatListInfo(TransItemDataInfo.GiveEquipItemInfo, star2, upgradeStarFailBonus2);
		BeforeGetAwakenStatInfo = new AwakenStatListInfo(TransItemDataInfo.GetEquipItemInfo, star2, upgradeStarFailBonus2);
		AfterGetAwakenStatInfo = new AwakenStatListInfo(TransItemDataInfo.GetEquipItemInfo, star, upgradeStarFailBonus);
	}

	private void InitSoulWeaponStatListInfo()
	{
		BeforeGiveSoulWeaponStatInfo = new SoulWeaponStatListInfo(TransItemDataInfo.GiveEquipItemInfo.SoulWeaponInfo);
		AfterGiveSoulWeaponStatInfo = new SoulWeaponStatListInfo(TransItemDataInfo.GetEquipItemInfo.SoulWeaponInfo);
		BeforeGetSoulWeaponStatInfo = new SoulWeaponStatListInfo(TransItemDataInfo.GetEquipItemInfo.SoulWeaponInfo);
		AfterGetSoulWeaponStatInfo = new SoulWeaponStatListInfo(TransItemDataInfo.GiveEquipItemInfo.SoulWeaponInfo);
	}

	private void SetupTransitionInfo(EquipItemInfo equipItemInfo)
	{
		if (_transitionType == EquipTransitionType.Awaken)
		{
			SetupAwakenTransitionInfo(equipItemInfo);
		}
		else if (_transitionType == EquipTransitionType.SoulWeapon)
		{
			SetupSoulWeaponTransitionInfo(equipItemInfo);
		}
	}

	private void SetupAwakenTransitionInfo(EquipItemInfo equipItemInfo)
	{
		bool flag = (SelectedGetItem = equipItemInfo.SlotIndex == TransItemDataInfo.GetEquipItemInfo.SlotIndex);
		SetEquipItemIconSelected(flag);
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		AwakenStatListInfo awakenStatListInfo = (flag ? BeforeGetAwakenStatInfo : BeforeGiveAwakenStatInfo);
		AwakenStatListInfo obj = (flag ? AfterGetAwakenStatInfo : AfterGiveAwakenStatInfo);
		int star = awakenStatListInfo.Star;
		int star2 = obj.Star;
		_statListComponent.AddAwakenChange(star, star2, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		int failBonus = awakenStatListInfo.FailBonus;
		int failBonus2 = obj.FailBonus;
		_statListComponent.AddFailBonusChange(failBonus, failBonus2, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		StatData itemStat = awakenStatListInfo.ItemStat;
		StatData itemStat2 = obj.ItemStat;
		bool flag3 = _statListComponent.AddStatChangeColorInfos(itemStat, itemStat2, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		int num = 1;
		while (true)
		{
			int fixedOptionIndex = data.GetSubData<EquipItemData>().GetFixedOptionIndex(num);
			if (fixedOptionIndex == 0)
			{
				break;
			}
			bool flag4 = _statListComponent.AddOptionChangeSkill(fixedOptionIndex, star, star2, StatListComponent.EOptionChangeTemplate.ForStarUpgrade, ColorInfos);
			flag3 = flag3 || flag4;
			num++;
		}
		foreach (EquipItemOptionInfo optionInfo in equipItemInfo.GetOptionInfoList())
		{
			_statListComponent.AddOptionChangeColorInfos(optionInfo, star, star2, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		}
		StatChangeScrollView.ResetPosition();
	}

	private void SetupSoulWeaponTransitionInfo(EquipItemInfo equipItemInfo)
	{
		bool flag = (SelectedGetItem = equipItemInfo.SlotIndex == TransItemDataInfo.GetEquipItemInfo.SlotIndex);
		SetEquipItemIconSelected(flag);
		ClearStatChange();
		_statListComponent = StatListComponent.Create(StatChangePanel, 2);
		SoulWeaponStatListInfo beforeStatInfo = (flag ? BeforeGetSoulWeaponStatInfo : BeforeGiveSoulWeaponStatInfo);
		SoulWeaponStatListInfo afterStatInfo = (flag ? AfterGetSoulWeaponStatInfo : AfterGiveSoulWeaponStatInfo);
		string stringDirect = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_UPGRADE");
		_statListComponent.AddIntegerStatChange(stringDirect, beforeStatInfo.Grade, afterStatInfo.Grade, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect2 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_REINFORCE");
		_statListComponent.AddIntegerStatChange(stringDirect2, beforeStatInfo.ReinforceLevel, afterStatInfo.ReinforceLevel, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: false, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect3 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ETHER_FIGURE");
		string text = "";
		text = ((!beforeStatInfo.IsSoulWeapon()) ? "-/-" : $"{beforeStatInfo.CurrentExp}/{beforeStatInfo.RequiredExp}");
		string text2 = "";
		text2 = ((!afterStatInfo.IsSoulWeapon()) ? "-/-" : $"{afterStatInfo.CurrentExp}/{afterStatInfo.RequiredExp}");
		_statListComponent.AddCustomStringFormatStatChange(stringDirect3, text, text2, delegate
		{
			if (beforeStatInfo.RequiredExp == afterStatInfo.RequiredExp)
			{
				return beforeStatInfo.CurrentExp.CompareTo(afterStatInfo.CurrentExp);
			}
			if (!beforeStatInfo.IsSoulWeapon())
			{
				return 1;
			}
			return (!afterStatInfo.IsSoulWeapon()) ? (-1) : 0;
		}, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
		string stringDirect4 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_REINFORCE_SUCCESS");
		float before = ((beforeStatInfo.SuccessRate > 100f) ? 100f : beforeStatInfo.SuccessRate);
		float after = ((afterStatInfo.SuccessRate > 100f) ? 100f : afterStatInfo.SuccessRate);
		_statListComponent.AddFloatStatChange(stringDirect4, before, after, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: true, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect5 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_ATTACK");
		_statListComponent.AddFloatStatChange(stringDirect5, beforeStatInfo.SoulWeaponAtkRate, afterStatInfo.SoulWeaponAtkRate, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: true, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect6 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_HP");
		_statListComponent.AddFloatStatChange(stringDirect6, beforeStatInfo.SoulWeaponHpRate, afterStatInfo.SoulWeaponHpRate, StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos, isPercentage: true, beforeStatInfo.IsSoulWeapon(), "-", afterStatInfo.IsSoulWeapon(), "-");
		string stringDirect7 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_SKILL");
		string stringDirect8 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_POSSIBLE");
		string stringDirect9 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_IMPOSSIBLE");
		string before2 = (beforeStatInfo.CanUseSkill ? stringDirect8 : stringDirect9);
		string after2 = (afterStatInfo.CanUseSkill ? stringDirect8 : stringDirect9);
		_statListComponent.AddCustomStringFormatStatChange(stringDirect7, before2, after2, () => afterStatInfo.CanUseSkill.CompareTo(beforeStatInfo.CanUseSkill), StatListComponent.EStatChangeTemplate.ForStarUpgrade, ColorInfos);
	}

	public void SetTransitionComplete(int giveItemSlotIndex, int getItemSlotIndex)
	{
		SetLabelTexts();
		EquipItemInfo equipItemBySlotIndex = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(giveItemSlotIndex);
		EquipItemInfo equipItemBySlotIndex2 = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(getItemSlotIndex);
		SetEquipItemIcon(equipItemBySlotIndex, equipItemBySlotIndex2);
		SetEquipItemIconSelected(SelectedGetItem);
	}

	private void SetEquipItemIconSelected(bool isGetEquipItemSelect)
	{
		TransItemInfo.SetGiveEquipItemIconSelected(!isGetEquipItemSelect);
		TransItemInfo.SetGetEquipItemIconSelected(isGetEquipItemSelect);
	}

	public override void Close()
	{
		if (CurrentState != TransitionState.TransitionEffectPlaying)
		{
			TransItemInfo.ClearEquipItemIcons();
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

	public void SetTransitionEffectPlay(Action setTransitionResult)
	{
		if (!(TransitionEffect == null) && !TransitionEffect.IsPlaying)
		{
			VespaCoroutine.Start(coTransitionEffectPlay(setTransitionResult));
		}
	}

	private IEnumerator coTransitionEffectPlay(Action setTransitionResult)
	{
		CurrentState = TransitionState.TransitionEffectPlaying;
		TransitionEffect.Play(delegate
		{
			CurrentState = TransitionState.TransitionComplete;
			SetColliderEnable(isEnable: true);
		});
		yield return new WaitForSeconds(TransitionResultSetTime);
		Vespa_System_Action_Extension_Method.fire(setTransitionResult);
	}

	public void SetColliderEnable(bool isEnable)
	{
		UIHelper.EnableCollider(Button_Close, isEnable);
		UIHelper.EnableCollider(DragScrollView, isEnable);
		UIHelper.EnableCollider(Button_Transition, isEnable);
	}

	public void OnClickTransition()
	{
		if (_listener == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("OPTION_TAP_PUSH");
		string stringDirect2 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_CONFIRM_NOTI_DESC");
		switch (CurrentState)
		{
		case TransitionState.TransitionConfirm:
			if (_transitionType == EquipTransitionType.Awaken)
			{
				SetColliderEnable(isEnable: false);
				int index = TransItemDataInfo.UseItemData.Index;
				int slotIndex = TransItemDataInfo.GiveEquipItemInfo.SlotIndex;
				int slotIndex2 = TransItemDataInfo.GetEquipItemInfo.SlotIndex;
				_listener.OnClickTransition(index, slotIndex, slotIndex2, _transitionType);
				break;
			}
			MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
			{
				SetColliderEnable(isEnable: false);
				int index2 = TransItemDataInfo.UseItemData.Index;
				int slotIndex3 = TransItemDataInfo.GiveEquipItemInfo.SlotIndex;
				int slotIndex4 = TransItemDataInfo.GetEquipItemInfo.SlotIndex;
				_listener.OnClickTransition(index2, slotIndex3, slotIndex4, _transitionType);
			}, null);
			break;
		case TransitionState.TransitionComplete:
			Close();
			break;
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		SetupTransitionInfo(itemIconComponent.EquipItemInfo);
	}
}
