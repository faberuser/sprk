using System.Collections;
using System.Collections.Generic;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NPayShop;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public abstract class PayShopBaseInfoPopup : BaseWindow
{
	public interface IListener
	{
		void OnBuy(int productIndex);

		void OnClickPurchaseLaw();

		void OnClickSpecialCommercialTransactionsLaw();

		void OnClickMoneySettlementLaw();

		void OnClickProbability(int productIndex);

		void OnClickRightAwayProbability(NewPayShopProductInfo productInfo);

		void OnBuyRightAwayProduct(NewPayShopProductInfo productInfo);
	}

	public UILabel Label_Title;

	public UIBundleLoader ProductTexture;

	public UIScrollView TagScrollView;

	public Transform TagPos;

	public UILabel Label_PurchasableCount;

	public UILabel Label_Period;

	public UIButton Button_Buy;

	public PayShopPriceComponent PriceComponent;

	public Vector2 TagPadding;

	public UIWidget Widget_LawRoot;

	public UIWidget Widget_JapanLawRoot;

	public UIWidget Widget_NormalLawRoot;

	public UIButton Button_Probability;

	public UILabel Label_Withdraw;

	public UIButton Button_NormalLawDetail;

	private List<PayShopTagComponent> _tagComponents = new List<PayShopTagComponent>();

	protected NewPayShopProductInfo _originInfo;

	protected NewPayShopProductInfo _currentInfo;

	protected IListener _listener;

	public virtual void Open(IListener listener, int index)
	{
		Open(delegate
		{
			_originInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(index);
			_listener = listener;
			Init(_originInfo);
		}, null);
	}

	public virtual void Open(IListener listener, int orginalIndex, int comboIndex)
	{
		Open(delegate
		{
			_originInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(orginalIndex);
			NewPayShopProductInfo productInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(comboIndex);
			_listener = listener;
			Init(productInfo);
		}, null);
	}

	public virtual void Open(IListener listener, NewPayShopProductInfo newPayShopProductInfo)
	{
		Open(delegate
		{
			_originInfo = newPayShopProductInfo;
			_listener = listener;
			Init(_originInfo);
		}, null);
	}

	protected virtual void Init(NewPayShopProductInfo productInfo)
	{
		_currentInfo = productInfo;
		PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productInfo.InAppPurchaseId, productInfo);
		SetProdcutTitle(productInfo);
		SetProductIcon(productInfo);
		SetPurcahsePeriod(productInfo);
		SetPurcahseableCount(productInfo, purchaseInfo);
		SetPriceComponent(productInfo, purchaseInfo, skuInfo);
		SetBuyButton(_originInfo, productInfo, PriceComponent, Button_Buy);
		SetLocalizedLawButton(productInfo);
		SetProbabilityButton(productInfo);
		SetTag(productInfo);
		SetLawRoot(productInfo);
		SetExtra(productInfo, purchaseInfo);
	}

	protected virtual void SetProdcutTitle(NewPayShopProductInfo productInfo)
	{
		PayShopUIHelper.SetProductTitle(productInfo, Label_Title);
	}

	protected virtual void SetProductIcon(NewPayShopProductInfo productInfo)
	{
		PayShopUIHelper.SetProductIcon(productInfo, ProductTexture);
	}

	protected virtual void SetPurcahsePeriod(NewPayShopProductInfo productInfo)
	{
		PayShopUIHelper.SetPurchasePeriod(productInfo, Label_Period);
	}

	protected virtual void SetPurcahseableCount(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		PayShopUIHelper.SetPurchseableCount(productInfo, purchaseInfo, Label_PurchasableCount);
	}

	protected virtual void SetPriceComponent(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, SkuInfo skuInfo)
	{
		PayShopUIHelper.SetPriceComponent(productInfo, purchaseInfo, skuInfo, PriceComponent);
	}

	private void SetTag(NewPayShopProductInfo productInfo)
	{
		NewPayShopTagData[] tags = GameRoot.Instance.NewPayShopTagDataContainer.GetTags(PayShopIndexType.PayShop, productInfo.Index);
		foreach (NewPayShopTagData tagData in tags)
		{
			PayShopTagComponent item = CreateTagComponent(tagData);
			_tagComponents.Add(item);
		}
		VespaCoroutine.Start(coTagReposition(_tagComponents));
	}

	private IEnumerator coTagReposition(List<PayShopTagComponent> lst)
	{
		yield return null;
		yield return null;
		yield return null;
		PayShopTagList.Reposition(lst, TagPadding, wordWrap: false);
		if (TagScrollView != null)
		{
			TagScrollView.ResetPosition();
		}
	}

	private PayShopTagComponent CreateTagComponent(NewPayShopTagData tagData)
	{
		PayShopTagComponent payShopTagComponent = PayShopTagComponent.PrefabAuto.Factory.Create(TagPos);
		payShopTagComponent.Open(tagData, null);
		return payShopTagComponent;
	}

	protected void SetLocalizedLawButton(NewPayShopProductInfo productInfo)
	{
		PayShopUIHelper.SetLocalizedLaw(productInfo, Widget_JapanLawRoot, Widget_NormalLawRoot, Label_Withdraw, Button_NormalLawDetail);
	}

	protected virtual bool GetEnableBuyButton(NewPayShopProductInfo originInfo, NewPayShopProductInfo productInfo, PayShopPriceComponent priceComponent)
	{
		if (productInfo != originInfo)
		{
			return false;
		}
		return GetBuyable(priceComponent);
	}

	protected virtual bool GetBuyable(PayShopPriceComponent priceComponent)
	{
		if (priceComponent == null)
		{
			return false;
		}
		return priceComponent.IsBuyable;
	}

	protected virtual void SetBuyButton(NewPayShopProductInfo originInfo, NewPayShopProductInfo productInfo, PayShopPriceComponent priceComponent, UIButton buyButton)
	{
		bool enableBuyButton = GetEnableBuyButton(originInfo, productInfo, priceComponent);
		UIHelper.EnableButton(buyButton, enableBuyButton, grayscaleIfDisable: true);
	}

	protected virtual void SetProbabilityButton(NewPayShopProductInfo productInfo)
	{
		UIHelper.EnableGameObject(Button_Probability, productInfo.ShowProbability);
	}

	protected virtual void SetLawRoot(NewPayShopProductInfo productInfo)
	{
		bool enable = !String_Extension_Method.IsNullOrEmpty(productInfo.InAppPurchaseId);
		UIHelper.EnableGameObject(Widget_LawRoot, enable);
	}

	protected virtual void SetExtra(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
	}

	public override void Reset()
	{
		base.Reset();
		_originInfo = null;
		_currentInfo = null;
		ReleaseComponent();
	}

	protected virtual void ReleaseComponent()
	{
		FactoryBase.ReleaseAndClearList(_tagComponents);
	}

	public virtual void OnClickBuy()
	{
		if (SprkSpecialShop.BeforeBuy(this) && _listener != null)
		{
			NewPayShopProductCategoryType o_value = NewPayShopProductCategoryType.None;
			EnumLookup<NewPayShopProductCategoryType>.TryParse(_currentInfo.Category, out o_value);
			if (o_value == NewPayShopProductCategoryType.RightAway)
			{
				_listener.OnBuyRightAwayProduct(_currentInfo);
			}
			else
			{
				_listener.OnBuy(_currentInfo.Index);
			}
		}
	}

	public void OnClickPurchaseLaw()
	{
		if (_listener != null)
		{
			_listener.OnClickPurchaseLaw();
		}
	}

	public virtual void OnClickProbability()
	{
		if (_listener != null)
		{
			if (_currentInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
			{
				_listener.OnClickRightAwayProbability(_currentInfo);
			}
			else
			{
				_listener.OnClickProbability(_currentInfo.Index);
			}
		}
	}

	public void OnClickSpecialCommercialTransactionsLaw()
	{
		if (_listener != null)
		{
			_listener.OnClickSpecialCommercialTransactionsLaw();
		}
	}

	public void OnClickMoneySettlementLaw()
	{
		if (_listener != null)
		{
			_listener.OnClickMoneySettlementLaw();
		}
	}
}
