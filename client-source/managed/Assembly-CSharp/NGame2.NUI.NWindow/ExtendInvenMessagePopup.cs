using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ExtendInvenMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/ExtendInvenMessagePopup";

	public const string PrefabPath2 = "Resources/Prefab2/Window/MessagePopup/ExtendInvenMessagePopup2";

	public UIWidget RootWidget;

	public UILabel PriceLabel;

	public UILabel LabelMessage;

	public UILabel LabelResultMessage;

	public UIInput InputCount;

	public UIButton ButtonPlus;

	public UIButton ButtonMinus;

	public UIButton ButtonMax;

	public LocalizeLinkData LocalLinkMessage = new LocalizeLinkData();

	public LocalizeLinkData LocalLinkMessage2 = new LocalizeLinkData();

	public LocalizeLinkData LocalLinkMessage3 = new LocalizeLinkData();

	public LocalizeLinkData LocalLinkMessage4 = new LocalizeLinkData();

	private Action<int, int> _onYes;

	private Action _onNo;

	private InventoryType _inventoryType;

	private int _curEquipInvenSize;

	private int _maxEquipInvenSize;

	private int _myExtendCount;

	private int _minCount;

	private int _maxCount;

	private int _currentCount;

	private int _purchasableCount;

	public static void Open(Action<int, int> onYes, Action onNo = null, InventoryType inventoryType = InventoryType.Inventory)
	{
		WindowPrefab<ExtendInvenMessagePopup> windowPrefab = new WindowPrefab<ExtendInvenMessagePopup>("Resources/Prefab2/Window/MessagePopup/ExtendInvenMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes, onNo, inventoryType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open2(Action<int, int> onYes, Action onNo = null, InventoryType inventoryType = InventoryType.Inventory)
	{
		WindowPrefab<ExtendInvenMessagePopup> windowPrefab = new WindowPrefab<ExtendInvenMessagePopup>("Resources/Prefab2/Window/MessagePopup/ExtendInvenMessagePopup2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes, onNo, inventoryType);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(Action<int, int> onYes, Action onNo, InventoryType inventoryType)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		_inventoryType = inventoryType;
		Init();
	}

	private void Init()
	{
		InitValue();
		SetNumber(_minCount);
	}

	private void InitValue()
	{
		int extendMaxCount = GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount(_inventoryType);
		if (_inventoryType == InventoryType.Inventory)
		{
			_curEquipInvenSize = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
			_maxEquipInvenSize = Helper.GetInventoryExtendSize(extendMaxCount);
			_myExtendCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.InventoryExtend;
		}
		else if (_inventoryType == InventoryType.Chest)
		{
			_curEquipInvenSize = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
			_maxEquipInvenSize = Helper.GetChestExtendSize(extendMaxCount);
			_myExtendCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.ChestExtend;
		}
		else if (_inventoryType == InventoryType.PunishmentRune)
		{
			_curEquipInvenSize = GlobalAutoBehaviour<AccountManager>.instance.GetMaxPunishmentRuneItemCount();
			_maxEquipInvenSize = Helper.GetPunishmentExtendSize(extendMaxCount);
			_myExtendCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.PunishmentRuneStorageSlotExtend;
		}
		_minCount = 1;
		_maxCount = extendMaxCount - _myExtendCount;
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		_purchasableCount = _maxCount;
		for (int i = _minCount; i <= _maxCount; i++)
		{
			int totalPrice = GetTotalPrice(i);
			long num = totalGem;
			int num2 = totalPrice;
			if (num2 > num)
			{
				_purchasableCount = Mathf.Clamp(i - 1, _minCount, _maxCount);
				break;
			}
		}
	}

	private void SetNumber(int number)
	{
		_currentCount = Mathf.Clamp(number, _minCount, _maxCount);
		InputCount.value = _currentCount.ToString();
		UpdateCountButtonState();
		SetLabel();
	}

	private void UpdateCountButtonState()
	{
		bool enable = _currentCount < _maxCount;
		bool enable2 = _currentCount > _minCount;
		UIHelper.EnableButton(ButtonPlus, enable);
		UIHelper.EnableButton(ButtonMinus, enable2);
	}

	public void SetLabel()
	{
		int num = _myExtendCount + _currentCount;
		bool flag = false;
		InventoryExtendData inventoryExtendData = null;
		foreach (InventoryExtendData extendData in GameRoot.Instance.InventoryExtendDataContainer.GetExtendDataList(_inventoryType))
		{
			if (extendData.EquipItemExtendCount >= num)
			{
				inventoryExtendData = extendData;
				break;
			}
			flag = true;
		}
		if (inventoryExtendData == null)
		{
			VespaDebug.LogError("invenExtendData == null");
			return;
		}
		int equipItemExtendSize = inventoryExtendData.EquipItemExtendSize;
		_ = inventoryExtendData.EquipItemExtendPrice;
		string[] array = new string[4] { "", "", "", "" };
		if (_inventoryType == InventoryType.Inventory)
		{
			array[0] = LocalLinkMessage.GetLocalize(equipItemExtendSize);
			array[1] = LocalLinkMessage2.GetLocalize(_maxEquipInvenSize);
			array[2] = LocalLinkMessage3.GetLocalize(Helper.GetInventoryExtendSize(num), _maxEquipInvenSize);
			array[3] = ((!flag) ? SprkInventoryExpansionCostText(LocalizationManager.GetStringDirect("EQUIP_INVENTORY_EXPAND_COST1"), _maxEquipInvenSize) : SprkInventoryExpansionCostText(LocalizationManager.GetStringDirect("EQUIP_INVENTORY_EXPAND_COST2"), _maxEquipInvenSize));
		}
		else if (_inventoryType == InventoryType.Chest)
		{
			array[0] = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_MESSAGE_CHEST", equipItemExtendSize);
			array[1] = LocalLinkMessage2.GetLocalize(_maxEquipInvenSize);
			array[2] = LocalizationManager.GetStringDirect("EQUIP_ASK_EXTEND_MESSAGE_CHEST2", Helper.GetChestExtendSize(num), _maxEquipInvenSize);
		}
		else if (_inventoryType == InventoryType.PunishmentRune)
		{
			array[0] = LocalizationManager.GetStringDirect("PUNISHMENTRUNE_ASK_EXTEND_MESSAGE_CHEST", equipItemExtendSize);
			array[1] = LocalLinkMessage2.GetLocalize(_maxEquipInvenSize);
			array[2] = LocalizationManager.GetStringDirect("PUNISHMENTRUNE_ASK_EXTEND_MESSAGE_CHEST2", Helper.GetPunishmentExtendSize(num), _maxEquipInvenSize);
		}
		string text = $"{array[0]}\n{array[1]}\n{array[2]}";
		UIHelper.SetLabelText(LabelMessage, text);
		UIHelper.SetLabelText(LabelResultMessage, array[3]);
		long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
		int totalPrice = GetTotalPrice(_currentCount);
		long num2 = totalPrice;
		UIHelper.SetLabelText(PriceLabel, Vespa_Int_Extension_Method.ToCommaString(num2));
		UIHelper.SetLabelColor(PriceLabel, (totalGem >= num2) ? Color.white : Color.red);
	}

	private int GetTotalPrice(int addedNum)
	{
		int num = 0;
		int num2 = 0;
		int num3 = _myExtendCount;
		int num4 = addedNum;
		IList<InventoryExtendData> extendDataList = GameRoot.Instance.InventoryExtendDataContainer.GetExtendDataList(_inventoryType);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(extendDataList); i++)
		{
			if (num4 <= 0)
			{
				break;
			}
			InventoryExtendData inventoryExtendData = extendDataList[i];
			num2 = inventoryExtendData.EquipItemExtendCount - num3;
			if (num2 >= 0)
			{
				int num5 = ((num4 <= num2) ? num4 : num2);
				num += num5 * inventoryExtendData.EquipItemExtendPrice;
				num4 -= num5;
				num3 = inventoryExtendData.EquipItemExtendCount;
			}
		}
		return num;
	}

	public int GetNumber()
	{
		int result = 1;
		string value = InputCount.value;
		if (!string.IsNullOrEmpty(value))
		{
			int.TryParse(value, out result);
			result = Mathf.Clamp(result, 0, _maxCount);
		}
		return result;
	}

	private void ValidateNumber()
	{
		int number = GetNumber();
		SetNumber(number);
	}

	public void OnNumberChanged()
	{
		if (!string.IsNullOrEmpty(InputCount.value))
		{
			ValidateNumber();
		}
	}

	public void OnNumberSubmit()
	{
		ValidateNumber();
	}

	public void OnClickPlus()
	{
		SetNumber(GetNumber() + 1);
	}

	public void OnClickMinus()
	{
		SetNumber(GetNumber() - 1);
	}

	public void OnClickMax()
	{
		SetNumber(_purchasableCount);
	}

	public void OnClickShop()
	{
		OnClickNo();
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Convenience.ToString());
	}

	public void OnClickYes()
	{
		Action<int, int> onYes = _onYes;
		base.Close();
		int i_argA = 0;
		switch (_inventoryType)
		{
		case InventoryType.Inventory:
			i_argA = Helper.GetInventoryExtendSize(_currentCount, addBaseCount: false);
			break;
		case InventoryType.Chest:
			i_argA = Helper.GetChestExtendSize(_currentCount, addBaseCount: false);
			break;
		case InventoryType.PunishmentRune:
			i_argA = Helper.GetPunishmentExtendSize(_currentCount, addBaseCount: false);
			break;
		}
		int totalPrice = GetTotalPrice(_currentCount);
		Vespa_System_Action_Extension_Method.fire(onYes, i_argA, totalPrice);
	}

	public void OnClickNo()
	{
		Action onNo = _onNo;
		base.Close();
		Vespa_System_Action_Extension_Method.fire(onNo);
	}

	private static string SprkInventoryExpansionCostText(string original, int maximum)
	{
		return original.Replace("1000", maximum.ToString());
	}
}
