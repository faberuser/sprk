using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipEnchantOptionScene : EquipAwakenScene
{
	public interface Listener
	{
		void SelectEquipEnchantOption(EquipEnchantOptionScene caller, int optionIndex, EnchantOptionSelectResult result);

		void AgainEnchantOptionRenew(int equipItemSlotIndex, ItemData itemDataToEnchantEquipOption, int optionSlotIndex);
	}

	public enum EnchantOptionSelectResult
	{
		None,
		Old,
		New
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/FullScene/EquipEnchantOptionScene";

	public Transform EnchantOptionSelectRoot;

	public Transform OldEnchantOptionSelectRoot;

	public StatGaugeInfo OldStat;

	public Transform NewEnchantOptionSelectRoot;

	public StatGaugeInfo NewStat;

	public UIButton ConfirmSelectButton;

	public UIWidget OldResultOptionRoot;

	public UIWidget NewResultOptionRoot;

	public Transform ChangedOptionArrowRoot;

	private Listener _listener;

	private EnchantOptionSelectResult _selectResult;

	private StatListComponent _oldOptionListComponent;

	private StatListComponent _newOptionListComponent;

	private int _oldOptionIndex;

	private int _oldOptionStep;

	private GameObject _arrowFollowTarget;

	private bool _RenewEnchantOption;

	public ItemData EquipEnchantOptionItemData { get; private set; }

	public void Open(Listener listener, ItemData equipEnchantOptionItemData, EquipItemInfo enchantEquipItemInfo)
	{
		if (enchantEquipItemInfo == null)
		{
			_RenewEnchantOption = true;
			int renewEnchantOptionEquipItemSlotIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionEquipItemSlotIndex;
			enchantEquipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(renewEnchantOptionEquipItemSlotIndex);
			if (enchantEquipItemInfo == null)
			{
				return;
			}
		}
		else
		{
			_RenewEnchantOption = false;
		}
		EquipEnchantOptionItemData = equipEnchantOptionItemData;
		Open(enchantEquipItemInfo, null, CinemaType.Equip_Enchant, isSuccess: true, null);
		_listener = listener;
		if (_RenewEnchantOption)
		{
			byte renewEnchantOptionSlotIndex = enchantEquipItemInfo.RenewEnchantOptionSlotIndex;
			_oldOptionIndex = enchantEquipItemInfo.GetEnchantOptionIndex(renewEnchantOptionSlotIndex);
			_oldOptionStep = enchantEquipItemInfo.GetEnchantOptionStep(renewEnchantOptionSlotIndex);
		}
		UIHelper.EnableGameObject(EnchantOptionSelectRoot, enable: false);
	}

	public override void Reset()
	{
		base.Reset();
		ClearOptionStat();
		_selectResult = EnchantOptionSelectResult.None;
		_listener = null;
		_oldOptionIndex = 0;
		_oldOptionStep = 0;
		_arrowFollowTarget = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateArrowPosition();
	}

	private void ClearOptionStat()
	{
		FactoryBase.SafeReleaseComponent(ref _oldOptionListComponent);
		FactoryBase.SafeReleaseComponent(ref _newOptionListComponent);
	}

	public void OnSelectOldOption()
	{
		_selectResult = EnchantOptionSelectResult.Old;
		UpdateSelectButtonState();
	}

	public void OnSelectNewOption()
	{
		_selectResult = EnchantOptionSelectResult.New;
		UpdateSelectButtonState();
	}

	public void OnConfirmSelectOption()
	{
		if (_selectResult != EnchantOptionSelectResult.None && _listener != null)
		{
			byte renewEnchantOptionSlotIndex = base.EquipItemInfo.RenewEnchantOptionSlotIndex;
			int enchantOptionIndex = base.EquipItemInfo.GetEnchantOptionIndex(renewEnchantOptionSlotIndex);
			int renewEnchantOptionResultIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultIndex;
			int optionIndex = ((_selectResult == EnchantOptionSelectResult.New) ? renewEnchantOptionResultIndex : enchantOptionIndex);
			_listener.SelectEquipEnchantOption(this, optionIndex, _selectResult);
		}
	}

	private void UpdateSelectButtonState()
	{
		if (_RenewEnchantOption)
		{
			UIHelper.EnableButton(ConfirmSelectButton, _selectResult != EnchantOptionSelectResult.None);
			UIHelper.EnableGameObject(OldEnchantOptionSelectRoot, _selectResult == EnchantOptionSelectResult.Old);
			UIHelper.EnableGameObject(NewEnchantOptionSelectRoot, _selectResult == EnchantOptionSelectResult.New);
		}
	}

	protected override void ShowAwakenResult()
	{
		if (_RenewEnchantOption)
		{
			UIHelper.EnableGameObject(EnchantOptionSelectRoot, enable: true);
			byte renewEnchantOptionSlotIndex = base.EquipItemInfo.RenewEnchantOptionSlotIndex;
			int enchantOptionIndex = base.EquipItemInfo.GetEnchantOptionIndex(renewEnchantOptionSlotIndex);
			int enchantOptionStep = base.EquipItemInfo.GetEnchantOptionStep(renewEnchantOptionSlotIndex);
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(enchantOptionIndex);
			int current = data?.GetOptionValue(0, enchantOptionStep) ?? 0;
			int max = data?.GetOptionValueMax(0) ?? 0;
			SprkOpenPreviousEnchantOption(OldStat, data?.OptionType ?? OperationType.None, current, max);
			int renewEnchantOptionResultIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultIndex;
			int renewEnchantOptionResultStep = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultStep;
			EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(renewEnchantOptionResultIndex);
			int current2 = data2?.GetOptionValue(0, renewEnchantOptionResultStep) ?? 0;
			int max2 = data2?.GetOptionValueMax(0) ?? 0;
			NewStat.Open(data2?.OptionType ?? OperationType.None, current2, max2);
		}
		else
		{
			ShowEnchantOptionResult();
		}
		UpdateSelectButtonState();
	}

	public void ShowEnchantOptionResult()
	{
		UIHelper.EnableGameObject(EnchantOptionSelectRoot, enable: false);
		base.ShowAwakenResult();
		ShowEnchantResultOption();
	}

	private void ShowEnchantResultOption()
	{
		_oldOptionListComponent = StatListComponent.Create(OldResultOptionRoot);
		_newOptionListComponent = StatListComponent.Create(NewResultOptionRoot);
		UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: false);
		StatListComponent.StatWidget<SingleStatInfo> statWidget = ((_oldOptionIndex != 0) ? _oldOptionListComponent.AddEnchantOptionStat(_oldOptionIndex, _oldOptionStep) : _oldOptionListComponent.AddEmptyEnchantOptionStat());
		if (statWidget != null && (bool)statWidget.GameObject)
		{
			_arrowFollowTarget = statWidget.GameObject;
			UIHelper.EnableGameObject(ChangedOptionArrowRoot, enable: true);
			UpdateArrowPosition();
		}
		int num = 1;
		while (true)
		{
			int enchantOptionIndex = base.EquipItemInfo.GetEnchantOptionIndex(num);
			if (enchantOptionIndex != 0)
			{
				int enchantOptionStep = base.EquipItemInfo.GetEnchantOptionStep(num);
				if (num == base.EquipItemInfo.RenewEnchantOptionSlotIndex)
				{
					_newOptionListComponent.AddEnchantOptionStat(enchantOptionIndex, enchantOptionStep, renew: true);
				}
				else
				{
					_newOptionListComponent.AddEnchantOptionStat(enchantOptionIndex, enchantOptionStep);
				}
				num++;
				continue;
			}
			break;
		}
	}

	private void UpdateArrowPosition()
	{
		if ((bool)ChangedOptionArrowRoot && (bool)_arrowFollowTarget)
		{
			Vector3 position = ChangedOptionArrowRoot.transform.position;
			float y = _arrowFollowTarget.transform.position.y;
			if (!Mathf.Approximately(position.y, y))
			{
				Vector3 position2 = new Vector3(position.x, y, position.z);
				ChangedOptionArrowRoot.transform.position = position2;
			}
		}
	}

	public void OnClickRenewAgain()
	{
		if (_listener != null)
		{
			_listener.AgainEnchantOptionRenew(base.EquipItemInfo.SlotIndex, EquipEnchantOptionItemData, base.EquipItemInfo.RenewEnchantOptionSlotIndex);
		}
	}

	private static void SprkOpenPreviousEnchantOption(StatGaugeInfo gauge, Enum statType, int current, int max)
	{
		if ((OperationType)(object)statType == OperationType.None)
		{
			gauge.OpenEmpty(LocalizationManager.GetString(EquipEnchant.EMPTY_ENCHANT_OPTION));
		}
		else
		{
			gauge.Open(statType, current, max);
		}
	}
}
