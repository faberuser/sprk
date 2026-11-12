using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipEnchantOption : EquipEnhanceBase, IUserObserver<ItemInfo>, IUserObserver, EnchantOptionItemComponent.Listener
{
	public interface Listener
	{
		void OnEnchantOption(EquipEnchantOption caller);

		void OpenLastEnchantResult(EquipEnchantOption caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Equip/EquipEnchantOption";

	public UILabel Label_UpperMsg;

	public UISimpleGrid MaterialItemGrid;

	public UILabel Label_NumOfMaterial;

	public UILabel Label_Price;

	public UIButton Button_Enchant;

	public Transform WidgetEnchantOptionItemList;

	public UIScrollGrid EnchantOptionItemListGrid;

	public UILabel Label_NoEnchantOptionItem;

	public UIPanel TargetEquipOptionPanel;

	public UIPanel EnchantOptionPanel;

	public UIScrollView EnchantOptionScrollView;

	private Listener _listener;

	private ItemIconComponent _SelectEnchantOptionItemComponent;

	private StatGaugeComponent _targetEquipItemStatGaugeComponent;

	private StatGaugeComponent _enchantOptionItemStatGaugeComponent;

	private List<EnchantOptionItemComponent> _ListEnchantOptionItemComponents = new List<EnchantOptionItemComponent>();

	public ItemData ItemDataToEnchantOption { get; private set; }

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		FactoryBase.SafeReleaseComponent(ref _SelectEnchantOptionItemComponent);
		FactoryBase.SafeReleaseComponent(ref _targetEquipItemStatGaugeComponent);
		FactoryBase.SafeReleaseComponent(ref _enchantOptionItemStatGaugeComponent);
		FactoryBase.ReleaseAndClearList(_ListEnchantOptionItemComponents);
		ItemDataToEnchantOption = null;
	}

	public void Init(Listener listener)
	{
		_listener = listener;
		InitBase();
		CheckLastEnchantResult();
		DeselectTargetEquipItem();
		SelectEnchantOptionItem(null);
		SetupReqGoldAndEnchantButton();
		SetupEnchantOptionItemComponent();
	}

	public static bool CanEnchant(EquipItemInfo equipItemInfo)
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		if (data == null)
		{
			return false;
		}
		short tier = data.SubData.Tier;
		int detailIndex = data.SubData.DetailIndex;
		if (!GameRoot.Instance.EquipEnchantableConditionDataContainer.EnchantOptionItemCodeExists(tier, detailIndex))
		{
			return false;
		}
		EquipEnchantableConditionData data2 = GameRoot.Instance.EquipEnchantableConditionDataContainer.GetData(tier, detailIndex);
		long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
		int num = 0;
		for (int i = 0; i < data2.EnchantOptionItemCodes.Length; i++)
		{
			string code = data2.EnchantOptionItemCodes[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode != null)
			{
				int itemCount = Helper.GetItemCount(dataByCode.Index);
				EquipEnchantOptionItemData data3 = GameRoot.Instance.EquipEnchantOptionItemDataContainer.GetData(dataByCode.Code);
				if (data3 != null && (itemCount >= data3.ReqCount || gold >= data3.ReqGold))
				{
					num++;
				}
			}
		}
		return num > 0;
	}

	public override bool CanEnhance(EquipItemInfo equipItemInfo)
	{
		return CanEnchant(equipItemInfo);
	}

	public override void OnTargetItemSelected(EquipItemInfo equipItemInfo)
	{
		base.OnTargetItemSelected(equipItemInfo);
		if (equipItemInfo == null)
		{
			UIHelper.EnableGameObject(HeroEquipToggle, enable: true);
			UIHelper.EnableGameObject(InventoryEquipToggle, enable: true);
			SelectEnchantOptionItem(null);
			OnEquipMenuToggleChanged();
		}
		else
		{
			UIHelper.EnableGameObject(HeroEquipToggle, enable: false);
			UIHelper.EnableGameObject(InventoryEquipToggle, enable: false);
			UIHelper.EnableGameObject(HeroEquipRoot, enable: false);
			UIHelper.EnableGameObject(InventoryEquipRoot, enable: false);
		}
		SetupTargetEquipItemOptionStat();
		SetupEnchantOptionItemList();
		SetupUpperMessage();
	}

	protected override ItemIconComponent CreateInventoryItemComponent(EquipItemInfo equipItemInfo, bool noTechnomagic = false)
	{
		if (CanEnhance(equipItemInfo))
		{
			return base.CreateInventoryItemComponent(equipItemInfo, noTechnomagic);
		}
		return null;
	}

	private void SetupEnchantOptionItemList()
	{
		FactoryBase.ReleaseAndClearList(_ListEnchantOptionItemComponents);
		if (base.SelectedEquipItemInfo == null)
		{
			UIHelper.EnableGameObject(WidgetEnchantOptionItemList, enable: false);
			return;
		}
		UIHelper.EnableGameObject(WidgetEnchantOptionItemList, enable: true);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(base.SelectedEquipItemInfo.ItemIndex);
		if (data == null)
		{
			VespaDebug.LogError("EquipItemData not found, itemIndex = " + base.SelectedEquipItemInfo.ItemIndex);
			UIHelper.EnableGameObject(WidgetEnchantOptionItemList, enable: false);
			return;
		}
		short tier = data.SubData.Tier;
		int detailIndex = data.SubData.DetailIndex;
		EquipEnchantableConditionData data2 = GameRoot.Instance.EquipEnchantableConditionDataContainer.GetData(tier, detailIndex);
		if (data2 == null)
		{
			return;
		}
		for (int i = 0; i < data2.EnchantOptionItemCodes.Length; i++)
		{
			string code = data2.EnchantOptionItemCodes[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode != null)
			{
				int itemCount = Helper.GetItemCount(dataByCode.Index);
				if (itemCount != 0)
				{
					EnchantOptionItemComponent enchantOptionItemComponent = UIHelper.AddGridItem(EnchantOptionItemListGrid, EnchantOptionItemComponent.PrefabAuto);
					enchantOptionItemComponent.Open(dataByCode, itemCount, this);
					_ListEnchantOptionItemComponents.Add(enchantOptionItemComponent);
				}
			}
		}
		UIHelper.RepositionGrid(EnchantOptionItemListGrid);
		UIHelper.EnableGameObject(Label_NoEnchantOptionItem, __Extension_Method_ICollection__.COUNT(_ListEnchantOptionItemComponents) == 0);
	}

	protected override void OnClickItem(ItemIconComponent component)
	{
		base.OnClickItem(component);
		SelectEnchantOptionItem(null);
	}

	void EnchantOptionItemComponent.Listener.OnClickComponent(EnchantOptionItemComponent comp)
	{
		if (!(comp == null) && comp.ItemDataToEnchantOption != null && comp.ItemDataToEnchantOption != ItemDataToEnchantOption)
		{
			SelectEnchantOptionItem(comp.ItemDataToEnchantOption);
		}
	}

	private void SelectEnchantOptionItemComponentOfItemData(ItemData itemData, bool selected)
	{
		EnchantOptionItemComponent enchantOptionItemComponent = _ListEnchantOptionItemComponents.Find((EnchantOptionItemComponent itemComp) => itemComp.ItemDataToEnchantOption != null && Vespa_Common_Class_Extension_Method.EQUALS(itemComp.ItemDataToEnchantOption, ItemDataToEnchantOption));
		if ((bool)enchantOptionItemComponent)
		{
			enchantOptionItemComponent.SetSelected(selected);
		}
	}

	private void SelectEnchantOptionItem(ItemData itemData)
	{
		if (ItemDataToEnchantOption != null)
		{
			SelectEnchantOptionItemComponentOfItemData(ItemDataToEnchantOption, selected: false);
			ItemDataToEnchantOption = null;
		}
		ItemDataToEnchantOption = itemData;
		if (ItemDataToEnchantOption != null)
		{
			SelectEnchantOptionItemComponentOfItemData(ItemDataToEnchantOption, selected: true);
		}
		SetupEnchantOptionItemComponent();
		SetupReqGoldAndEnchantButton();
		SetupEnchantOptionItemOptionStat();
		SetupUpperMessage();
	}

	private void SetupEnchantOptionItemComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _SelectEnchantOptionItemComponent);
		UIHelper.EnableGameObject(Label_NumOfMaterial, enable: false);
		if (ItemDataToEnchantOption == null)
		{
			return;
		}
		int itemCount = Helper.GetItemCount(ItemDataToEnchantOption.Index);
		if (itemCount != 0)
		{
			UIHelper.EnableGameObject(Label_NumOfMaterial, enable: true);
			_SelectEnchantOptionItemComponent = UIHelper.AddGridItem(MaterialItemGrid, ItemIconComponent.PrefabAuto);
			_SelectEnchantOptionItemComponent.Open(ItemDataToEnchantOption, this);
			int num = 1;
			EquipEnchantOptionItemData data = GameRoot.Instance.EquipEnchantOptionItemDataContainer.GetData(ItemDataToEnchantOption.Code);
			if (data != null)
			{
				num = data.ReqCount;
			}
			UIHelper.SetLabelText(Label_NumOfMaterial, itemCount + "/" + num);
		}
	}

	private void SetupReqGoldAndEnchantButton()
	{
		int num = 0;
		long num2 = num;
		if (ItemDataToEnchantOption != null)
		{
			EquipEnchantOptionItemData data = GameRoot.Instance.EquipEnchantOptionItemDataContainer.GetData(ItemDataToEnchantOption.Code);
			if (data != null)
			{
				int reqGold = data.ReqGold;
				num2 = reqGold;
			}
		}
		long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold;
		string text = Vespa_Int_Extension_Method.ToCommaString(num2);
		long num3 = num2;
		int num4 = 0;
		if (num3 > num4 && num2 > gold)
		{
			text = NGUIText.EncodeColor(text, Color.red);
		}
		UIHelper.SetLabelText(Label_Price, text);
		UIButton button_Enchant = Button_Enchant;
		long num5 = num2;
		int num6 = 0;
		UIHelper.EnableButton(button_Enchant, num5 > num6 && num2 <= gold);
	}

	private void SetupTargetEquipItemOptionStat()
	{
		FactoryBase.SafeReleaseComponent(ref _targetEquipItemStatGaugeComponent);
		if (base.SelectedEquipItemInfo == null)
		{
			return;
		}
		_targetEquipItemStatGaugeComponent = StatGaugeComponent.DisplayPrefabAuto.Factory.Create(TargetEquipOptionPanel.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_targetEquipItemStatGaugeComponent.transform);
		_targetEquipItemStatGaugeComponent.SetTitle(null);
		if (!base.SelectedEquipItemInfo.EnchantOptionExists())
		{
			_targetEquipItemStatGaugeComponent.AddEmptyStat(LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NOT_EXIST));
		}
		int num = 1;
		while (true)
		{
			int enchantOptionIndex = base.SelectedEquipItemInfo.GetEnchantOptionIndex(num);
			if (enchantOptionIndex == 0)
			{
				break;
			}
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(enchantOptionIndex);
			if (data != null)
			{
				int optionValueStep = data.OptionValueStep;
				int currentStep = Mathf.Min(base.SelectedEquipItemInfo.GetEnchantOptionStep(num), optionValueStep);
				OperationType optionType = data.OptionType;
				int optionValue = data.GetOptionValue(0, currentStep);
				int optionValueMax = data.GetOptionValueMax(0);
				_targetEquipItemStatGaugeComponent.AddStat(optionType, optionValue, optionValueMax);
			}
			num++;
		}
		if (EnchantOptionScrollView != null)
		{
			EnchantOptionScrollView.ResetPosition();
		}
	}

	private void SetupEnchantOptionItemOptionStat()
	{
		FactoryBase.SafeReleaseComponent(ref _enchantOptionItemStatGaugeComponent);
		if (ItemDataToEnchantOption == null)
		{
			return;
		}
		EquipEnchantOptionItemData data = GameRoot.Instance.EquipEnchantOptionItemDataContainer.GetData(ItemDataToEnchantOption.Code);
		if (data == null)
		{
			return;
		}
		_enchantOptionItemStatGaugeComponent = StatGaugeComponent.DisplayPrefabAuto.Factory.Create(EnchantOptionPanel.transform);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_enchantOptionItemStatGaugeComponent.transform);
		_enchantOptionItemStatGaugeComponent.SetTitle(null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data.EnchantOptionIndex1); i++)
		{
			int num = data.EnchantOptionIndex1[i];
			if (num == 0)
			{
				break;
			}
			EquipOptionData data2 = GameRoot.Instance.EquipOptionDataContainer.GetData(num);
			if (data2 != null)
			{
				OperationType optionType = data2.OptionType;
				int optionValueMin = data2.GetOptionValueMin(0);
				int optionValueMax = data2.GetOptionValueMax(0);
				_enchantOptionItemStatGaugeComponent.AddStat(optionType, optionValueMin, optionValueMax, "~");
			}
		}
		if (EnchantOptionScrollView != null)
		{
			EnchantOptionScrollView.ResetPosition();
		}
	}

	public void OnClickEnchant()
	{
		if (_listener != null)
		{
			_listener.OnEnchantOption(this);
		}
	}

	private void CheckLastEnchantResult()
	{
		int renewEnchantOptionEquipItemSlotIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionEquipItemSlotIndex;
		if (renewEnchantOptionEquipItemSlotIndex == 0)
		{
			return;
		}
		int renewEnchantOptionResultIndex = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultIndex;
		if (renewEnchantOptionResultIndex == 0)
		{
			return;
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(renewEnchantOptionEquipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("Failed to find equip item of renew : equipItemSlotIndex=" + renewEnchantOptionEquipItemSlotIndex + " enchantOptionResultIndex=" + renewEnchantOptionResultIndex);
			return;
		}
		if (equipItemInfo.RenewEnchantOptionSlotIndex == 0)
		{
			VespaDebug.LogError("RenewEnchantOptionSlotIndex = 0 : equipItemSlotIndex=" + renewEnchantOptionEquipItemSlotIndex + " enchantOptionResultIndex=" + renewEnchantOptionResultIndex);
			return;
		}
		ItemIconComponent equipItemIconComponent = GetEquipItemIconComponent(renewEnchantOptionEquipItemSlotIndex);
		if (equipItemIconComponent != null)
		{
			OnClickEquipItem(equipItemIconComponent);
		}
		if (_listener != null)
		{
			_listener.OpenLastEnchantResult(this);
		}
	}

	private void SetupUpperMessage()
	{
		if (base.SelectedEquipItemInfo == null)
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipEnchant.SELECT_ITEM_TO_ENCHANT));
		}
		else if (ItemDataToEnchantOption == null)
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipEnchant.SELECT_ENCHANT_OPTION_ITEM));
		}
		else if (CanEnhance(base.SelectedEquipItemInfo))
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipEnchant.CAN_DO_ENCHANT_OPTION));
		}
		else
		{
			UIHelper.SetLabelText(Label_UpperMsg, LocalizationManager.GetString(EquipEnchant.CANNOT_DO_ENCHANT_OPTION));
		}
	}

	public override void OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		base.OnChanged(changedInfo);
		if (changedInfo != null && changedInfo.EquipItemInfo != null && changedInfo.EquipItemInfo == base.SelectedEquipItemInfo)
		{
			bool flag = CanEnhance(changedInfo.EquipItemInfo);
			OnTargetItemSelected(flag ? changedInfo.EquipItemInfo : null);
			if (_targetItemIconComponent != null)
			{
				_targetItemIconComponent.Refresh();
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && ItemDataToEnchantOption != null && ItemDataToEnchantOption.Index == changedInfo.ItemIndex)
		{
			SetupReqGoldAndEnchantButton();
			SetupEnchantOptionItemList();
			SetupEnchantOptionItemComponent();
		}
	}
}
