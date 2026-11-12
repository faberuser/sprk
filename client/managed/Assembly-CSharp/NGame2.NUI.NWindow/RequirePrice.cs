using System;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class RequirePrice
{
	public UILabel ReqPriceLabel;

	public UIWidget IconProxyRootWidget;

	public CurrencyType ReqPriceCurrencyType;

	public bool UsePriceColor;

	public Color DefaultPriceColor = Color.white;

	public Color LackPriceColor = Color.red;

	private IconProxy _iconProxy;

	private long _myCurrencyAmount;

	public long ReqPrice { get; private set; }

	public void Open(long reqPrice, long myCurrencyAmount = 0L)
	{
		Init(reqPrice, myCurrencyAmount);
	}

	public void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
	}

	private void Init(long reqPrice, long myCurrencyAmount)
	{
		SetPrice(reqPrice, myCurrencyAmount);
		SetReqPriceLabel();
		SetReqPriceCurrencyIcon();
	}

	public void SetPrice(long reqPrice, long myCurrencyAmount = 0L)
	{
		ReqPrice = reqPrice;
		_myCurrencyAmount = myCurrencyAmount;
		SetReqPriceLabel();
	}

	private void SetReqPriceLabel()
	{
		UIHelper.SetLabelText(ReqPriceLabel, (ReqPrice < 0) ? "-" : Vespa_Long_Extension_Method.ToCommaString(ReqPrice));
		SetReqPriceLabelColor();
	}

	public void SetLabelColor(Color color)
	{
		UIHelper.SetLabelColor(ReqPriceLabel, color);
	}

	private void SetReqPriceLabelColor()
	{
		if (UsePriceColor)
		{
			UIHelper.SetLabelColor(ReqPriceLabel, (_myCurrencyAmount >= ReqPrice) ? DefaultPriceColor : LackPriceColor);
		}
	}

	private void SetReqPriceCurrencyIcon()
	{
		if (!(IconProxyRootWidget == null))
		{
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconProxyRootWidget);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(ReqPriceCurrencyType);
			}
		}
	}
}
