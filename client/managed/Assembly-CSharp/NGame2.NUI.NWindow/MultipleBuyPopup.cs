using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NShop;
using NGame2.NUI.NManager;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MultipleBuyPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MultipleBuyPopup";

	public UIWidget IconProxyRoot;

	public IconProxy IconProxy;

	public Transform ItemIconRoot;

	public UILabel Label_Desc;

	public UILabel Label_Cost;

	public UIInput Input_SelectCount;

	public UIButton Button_IncCount;

	public UIButton Button_DecCount;

	private Action<int> _onBuy;

	private ShopItemData _shopItemData;

	private ShopListItemInfo _shopListItemInfo;

	private ItemData _itemData;

	private ShopItemInfo _itemInfo;

	private ShopData _shopData;

	private ShopCostType _currentCostType;

	private long _price;

	private long _selectBuyCount = 1L;

	private ItemIconComponent _itemIconComponent;

	public static void Open(long price, ShopItemComponent selectedShopItem, ShopData shopData, Action<int> onBuy)
	{
		WindowPrefab<MultipleBuyPopup> windowPrefab = new WindowPrefab<MultipleBuyPopup>("Resources/Prefab2/Window/MessagePopup/MultipleBuyPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(price, selectedShopItem, shopData, onBuy);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(long price, ShopItemComponent selectedShopItem, ShopData shopData, Action<int> onBuy)
	{
		base.Open();
		_shopItemData = selectedShopItem.ShopItemData;
		_shopListItemInfo = selectedShopItem.ShopListItemInfo;
		_shopData = shopData;
		_currentCostType = selectedShopItem.GetCurrentCostType();
		if (_shopItemData != null)
		{
			_itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(_shopItemData.ItemCode);
			_itemInfo = SceneAutoBehaviour<ShopManagement>.instance.GetShopItemPurchaseInfo(_shopData.Index, _shopItemData.Index);
		}
		else if (_shopListItemInfo != null)
		{
			_itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(_shopListItemInfo.ItemCode);
			_itemInfo = null;
		}
		_onBuy = onBuy;
		_price = price;
		Init();
	}

	protected void Init()
	{
		int num = 1;
		_selectBuyCount = num;
		if (Input_SelectCount != null)
		{
			Input_SelectCount.selectAllTextOnFocus = true;
		}
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		_itemIconComponent.Open(_itemData);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		_itemIconComponent.gameObject.SetActive(value: true);
		string itemName = ItemNameBuilder.GetItemName(_itemData);
		long itemBuyMaxCount = GetItemBuyMaxCount();
		string text = "";
		text = ((_shopItemData.PurchasableCount <= 0) ? "NPC_SHOP_PRODUCT_BUY_POPUP_DESC" : "NPC_SHOP_PRODUCT_LIMIT_BUY_POPUP_DESC");
		string stringDirect = LocalizationManager.GetStringDirect(text, itemName, itemBuyMaxCount);
		UIHelper.SetLabelText(Label_Desc, stringDirect);
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconProxyRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(_currentCostType.ToString());
		}
		UpdateCountButtonState(inputUpdate: true);
	}

	protected long GetItemBuyMaxCount()
	{
		long num = ShopHelper.GetPlayerAssetValue(_currentCostType) / _price;
		if (_itemInfo != null && _shopItemData.PurchasableCount > 0)
		{
			int num2 = _shopItemData.PurchasableCount - _itemInfo.Purchased;
			long val = num;
			int num3 = num2;
			num = Math.Min(val, num3);
		}
		else if (_shopListItemInfo != null)
		{
			int num4 = _shopListItemInfo.PurchasableCount - _shopListItemInfo.Purchased;
			long val2 = num;
			int num5 = num4;
			num = Math.Min(val2, num5);
		}
		else if (_shopItemData != null && _shopItemData.PurchasableCount > 0)
		{
			long val3 = num;
			int purchasableCount = _shopItemData.PurchasableCount;
			num = Math.Min(val3, purchasableCount);
		}
		string text = "";
		text = ((_itemData.Type != ItemType.Equip) ? "NPCShopNormalItemLimit" : "NPCShopEquipItemLimit");
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt(text);
		if (valueInt >= 0)
		{
			long val4 = num;
			int num6 = valueInt;
			return Math.Min(num6, val4);
		}
		return num;
	}

	protected long GetCurrentCost()
	{
		return checked(_price * _selectBuyCount);
	}

	protected void UpdateCountButtonState(bool inputUpdate)
	{
		long itemBuyMaxCount = GetItemBuyMaxCount();
		int num = 1;
		UIHelper.EnableButton(Button_IncCount, _selectBuyCount < itemBuyMaxCount);
		UIButton button_DecCount = Button_DecCount;
		long selectBuyCount = _selectBuyCount;
		int num2 = num;
		UIHelper.EnableButton(button_DecCount, selectBuyCount > num2);
		if (inputUpdate && Input_SelectCount != null)
		{
			Input_SelectCount.value = _selectBuyCount.ToString();
		}
		UIHelper.SetLabelText(Label_Cost, Vespa_Long_Extension_Method.ToCommaString(GetCurrentCost()));
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		Clear();
	}

	protected void Clear()
	{
		_onBuy = null;
		_shopItemData = null;
		_shopListItemInfo = null;
		_itemData = null;
		_itemInfo = null;
		_shopData = null;
		int num = 0;
		_price = num;
		int num2 = 1;
		_selectBuyCount = num2;
	}

	public void OnClickIncCount()
	{
		long itemBuyMaxCount = GetItemBuyMaxCount();
		long selectBuyCount = _selectBuyCount;
		int num = 1;
		checked
		{
			long num2 = selectBuyCount + num;
			long num3 = itemBuyMaxCount;
			int min = 1;
			long num4 = num2;
			int num5 = Mathf.Clamp((int)num4, min, (int)num3);
			_selectBuyCount = num5;
			UpdateCountButtonState(inputUpdate: true);
		}
	}

	public void OnClickDecCount()
	{
		long itemBuyMaxCount = GetItemBuyMaxCount();
		long selectBuyCount = _selectBuyCount;
		int num = 1;
		checked
		{
			long num2 = selectBuyCount - num;
			long num3 = itemBuyMaxCount;
			int min = 1;
			long num4 = num2;
			int num5 = Mathf.Clamp((int)num4, min, (int)num3);
			_selectBuyCount = num5;
			UpdateCountButtonState(inputUpdate: true);
		}
	}

	public void OnClickMaxCount()
	{
		long itemBuyMaxCount = GetItemBuyMaxCount();
		_selectBuyCount = itemBuyMaxCount;
		UpdateCountButtonState(inputUpdate: true);
	}

	public void OnChangeInput_SelectCount()
	{
		if (!(Input_SelectCount == null))
		{
			long itemBuyMaxCount = GetItemBuyMaxCount();
			int value = Vespa_String_Extension_Method.TOINT(Input_SelectCount.value, 0);
			long num = itemBuyMaxCount;
			int num2 = Mathf.Clamp(value, 0, checked((int)num));
			_selectBuyCount = num2;
			UpdateCountButtonState(inputUpdate: true);
		}
	}

	public void OnSubmitInput_SelectCount()
	{
		if (!(Input_SelectCount == null))
		{
			long itemBuyMaxCount = GetItemBuyMaxCount();
			int value = Vespa_String_Extension_Method.TOINT(Input_SelectCount.value, 0);
			long num = itemBuyMaxCount;
			int num2 = Mathf.Clamp(value, 1, checked((int)num));
			_selectBuyCount = num2;
			UpdateCountButtonState(inputUpdate: true);
		}
	}

	public void OnClickBuy()
	{
		Action action = delegate
		{
			Action<int> onBuy = _onBuy;
			long selectBuyCount2 = _selectBuyCount;
			Vespa_System_Action_Extension_Method.fire(onBuy, checked((int)selectBuyCount2));
			Close();
		};
		long selectBuyCount = _selectBuyCount;
		int num = 1;
		if (selectBuyCount > num)
		{
			string itemName = ItemNameBuilder.GetItemName(_itemData);
			string stringDirect = LocalizationManager.GetStringDirect("NPC_SHOP_PRODUCT_CONFIRM_POPUP_DESC", itemName, _selectBuyCount);
			MultipleBuyConfirmPopup.Open(GetCurrentCost(), _currentCostType.ToString(), _itemData, stringDirect, action);
		}
		else if (_shopData.BeforeBuyConfirm)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.NLocalization.Shop.BUY_CONFIRM_MSG);
			MessagePopup.Open(text, message, action, null);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
	}

	public void OnClickCancel()
	{
		base.Close();
	}
}
