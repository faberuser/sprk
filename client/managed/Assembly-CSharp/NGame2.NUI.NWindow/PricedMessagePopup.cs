using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PricedMessagePopup : BaseWindow
{
	public enum EPriceType
	{
		None,
		Gold,
		Gem,
		RubyPoint
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PricedMessagePopup";

	public UIWidget RootWidget;

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UILabel PriceLabel;

	public UILabel ActionLabel;

	public UILabel OriginalPriceLabel;

	public UILabelAligner PriceLabelAligner;

	public UIWidget GoldWidget;

	public UIWidget GemWidget;

	public UIWidget EventWidget;

	public UIWidget RubyPointWidget;

	private Action _onYes;

	private Action _onNo;

	public static CurrencyType GetCurrencyType(EPriceType priceType)
	{
		return priceType switch
		{
			EPriceType.Gem => CurrencyType.Gem, 
			EPriceType.Gold => CurrencyType.Gold, 
			_ => CurrencyType.None, 
		};
	}

	public static void Open(string title, string message, string action, EPriceType priceType, long price, Action onYes, Action onNo = null)
	{
		CurrencyType currencyType = GetCurrencyType(priceType);
		long currencyCount = price;
		UIHelper.BuyConfirmPopupOpen(currencyType, currencyCount, delegate
		{
			Open(title, message, action, priceType, price, price, onYes, onNo);
		}, onYes);
	}

	public static void Open(string title, string message, string action, EPriceType priceType, long originalPrice, long discountPrice, Action onYes, Action onNo = null)
	{
		WindowPrefab<PricedMessagePopup> windowPrefab = new WindowPrefab<PricedMessagePopup>("Resources/Prefab2/Window/MessagePopup/PricedMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message, action, priceType, discountPrice, originalPrice, onYes, onNo);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string title, string message, string action, EPriceType priceType, long price, long originalPrice, Action onYes, Action onNo)
	{
		base.Open();
		_onYes = onYes;
		_onNo = onNo;
		UIHelper.SetLabelText(TitleLabel, title);
		UIHelper.SetLabelText(MessageLabel, message);
		if (price == 0L || price == originalPrice)
		{
			UIHelper.EnableGameObject(OriginalPriceLabel, enable: false);
			UIHelper.EnableGameObject(EventWidget, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(OriginalPriceLabel, enable: true);
			UIHelper.SetLabelText(OriginalPriceLabel, Vespa_Long_Extension_Method.ToCommaString(originalPrice));
			UIHelper.EnableGameObject(EventWidget, enable: true);
		}
		UIHelper.SetLabelText(PriceLabel, Vespa_Long_Extension_Method.ToCommaString(price));
		UIHelper.SetLabelText(ActionLabel, action);
		UIHelper.EnableGameObject(GoldWidget, priceType == EPriceType.Gold);
		UIHelper.EnableGameObject(GemWidget, priceType == EPriceType.Gem);
		UIHelper.EnableGameObject(RubyPointWidget, priceType == EPriceType.RubyPoint);
		if (PriceLabelAligner != null)
		{
			PriceLabelAligner.Align();
		}
	}

	public void OnClickYes()
	{
		if (!IsClosingWindowAnimating())
		{
			Action onYes = _onYes;
			_onYes = null;
			base.Close();
			Vespa_System_Action_Extension_Method.fire(onYes);
		}
	}

	public void OnClickNo()
	{
		if (!IsClosingWindowAnimating())
		{
			Action onNo = _onNo;
			_onNo = null;
			base.Close();
			Vespa_System_Action_Extension_Method.fire(onNo);
		}
	}
}
