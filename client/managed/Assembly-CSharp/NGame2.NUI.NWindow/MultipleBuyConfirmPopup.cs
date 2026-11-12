using System;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MultipleBuyConfirmPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MultipleBuyConfirmPopup";

	public UIWidget CurrencyIconProxyRoot;

	public IconProxy IconProxy;

	public Transform ItemIconRoot;

	public UILabel Label_Message;

	public UILabel Label_Price;

	private long _purchasePrice;

	private string _currencyName;

	private ItemData _itemData;

	private Action _onYes;

	private Action _onNo;

	private ItemIconComponent _itemIconComponent;

	public static void Open(long price, string currencyName, ItemData itemData, string message, Action onYes = null, Action onNo = null)
	{
		WindowPrefab<MultipleBuyConfirmPopup> windowPrefab = new WindowPrefab<MultipleBuyConfirmPopup>("Resources/Prefab2/Window/MessagePopup/MultipleBuyConfirmPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(price, currencyName, itemData, message, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(long price, string currencyName, ItemData itemData, string message, Action onYes, Action onNo)
	{
		base.Open();
		_purchasePrice = price;
		_currencyName = currencyName;
		_itemData = itemData;
		_onYes = onYes;
		_onNo = onNo;
		_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		_itemIconComponent.Open(_itemData);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		_itemIconComponent.gameObject.SetActive(value: true);
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(CurrencyIconProxyRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(_currencyName);
		}
		UIHelper.SetLabelText(Label_Message, message);
		UIHelper.SetLabelText(Label_Price, Vespa_Long_Extension_Method.ToCommaString(_purchasePrice));
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	public void OnClickYes()
	{
		if (_onYes != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onYes);
		}
		Close();
	}

	public void OnClickNo()
	{
		if (_onNo != null)
		{
			Vespa_System_Action_Extension_Method.fire(_onNo);
		}
		Close();
	}

	public void OnClickClose()
	{
		base.Close();
	}
}
