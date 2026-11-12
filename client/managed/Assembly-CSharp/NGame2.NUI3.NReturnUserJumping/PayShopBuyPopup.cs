using System;
using System.Collections.Generic;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NUI;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NPayShop;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NReturnUserJumping;

public class PayShopBuyPopup : BaseWindow
{
	public interface IListener
	{
		void OnBuy(int productIndex);

		void OnClickPurchaseLaw();

		void OnClickSpecialCommercialTransactionsLaw();

		void OnClickMoneySettlementLaw();

		void OnClickProbability(int productIndex);
	}

	[Serializable]
	public class MainFrameUiGroup
	{
		public UILabel Label_Title;

		public void Setup(NewPayShopProductInfo productInfo)
		{
			PayShopUIHelper.SetProductTitle(productInfo, Label_Title);
		}
	}

	[Serializable]
	public class PriceUiGroup
	{
		public UIButton Button_Buy;

		public PayShopPriceComponent PriceComponent;

		public void Setup(NewPayShopProductInfo productInfo)
		{
			PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
			SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(productInfo.InAppPurchaseId);
			PayShopUIHelper.SetPriceComponent(productInfo, purchaseInfo, skuInfo, PriceComponent);
			bool enable = false;
			if (PriceComponent != null)
			{
				enable = PriceComponent.IsBuyable;
			}
			UIHelper.EnableButton(Button_Buy, enable, grayscaleIfDisable: true);
		}
	}

	[Serializable]
	public class LawUiGroup
	{
		public UIWidget Widget_LawRoot;

		public UIWidget Widget_JapanLawRoot;

		public UIWidget Widget_NormalLawRoot;

		public UILabel Label_Withdraw;

		public UIButton Button_NormalLawDetail;

		public void Setup(NewPayShopProductInfo productInfo)
		{
			PayShopUIHelper.SetLocalizedLaw(productInfo, Widget_JapanLawRoot, Widget_NormalLawRoot, Label_Withdraw, Button_NormalLawDetail);
			bool enable = !String_Extension_Method.IsNullOrEmpty(productInfo.InAppPurchaseId);
			UIHelper.EnableGameObject(Widget_LawRoot, enable);
		}
	}

	[Serializable]
	public class ProductUiGroup
	{
		public UILabel Label_Desc;

		public UITable Table_Bonus;

		private List<PayShopProductSimpleComponent> _bonusList = new List<PayShopProductSimpleComponent>();

		public void ReleaseGroup()
		{
			FactoryBase.ReleaseAndClearList(_bonusList);
		}

		public void Setup(NewPayShopProductInfo productInfo)
		{
			UIHelper.SetLabelTextLocalizeDirect(Label_Desc, productInfo.ProductDetailDesc);
			AddCurrencyProduct(CurrencyType.LuaPoint, productInfo.LuaPoint);
			UIHelper.EnableGameObject(Table_Bonus, !__Extension_Method_ICollection__.IsNullOrEmpty(_bonusList));
			if (Table_Bonus != null && !__Extension_Method_ICollection__.IsNullOrEmpty(_bonusList))
			{
				Table_Bonus.Reposition();
			}
		}

		private void AddCurrencyProduct(CurrencyType currency, int value)
		{
			if (value > 0 && !(Table_Bonus == null))
			{
				PayShopProductSimpleComponent payShopProductSimpleComponent = PayShopProductSimpleComponent.PrefabAuto.Factory.Create(Table_Bonus.transform);
				if (!(payShopProductSimpleComponent == null))
				{
					ObtainItemType obtainType = GachaSelctShopHelper.GetObtainType(currency, isFree: false);
					ObtainItemType obtainType2 = obtainType;
					payShopProductSimpleComponent.Open(currency, value, obtainType2);
					_bonusList.Add(payShopProductSimpleComponent);
				}
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab3/ReturnUserJumping/PayShopBuyPopup";

	[Header("[[--- PayShopBuyPopup.MainFrameUiGroup ---]]")]
	public MainFrameUiGroup MainFrameUi;

	[Header("[[--- PayShopBuyPopup.PriceUiGroup ---]]")]
	public PriceUiGroup PriceUi;

	[Header("[[--- PayShopBuyPopup.LawUiGroup ---]]")]
	public LawUiGroup LawUi;

	[Header("[[--- PayShopBuyPopup.ProductUiGroup ---]]")]
	public ProductUiGroup ProductUi;

	private NewPayShopProductInfo _productInfo;

	public void Open(int index)
	{
		Open(delegate
		{
			NewPayShopProductInfo productInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(index);
			Init(productInfo);
		}, null);
	}

	private void Init(NewPayShopProductInfo productInfo)
	{
		_productInfo = productInfo;
		MainFrameUi.Setup(productInfo);
		PriceUi.Setup(productInfo);
		LawUi.Setup(productInfo);
		ProductUi.Setup(productInfo);
	}

	public override void Reset()
	{
		base.Reset();
		_productInfo = null;
		ReleaseComponent();
	}

	private void ReleaseComponent()
	{
		ProductUi.ReleaseGroup();
	}

	public void OnClickBuy()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.RequestPurchaseProduct<PayShopBuyPopup>(_productInfo.Index);
	}

	public void OnClickPurchaseLaw()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OnClickPurchaseLaw_Common();
	}

	public void OnClickSpecialCommercialTransactionsLaw()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OnClickSpecialCommercialTransactionsLaw_Common();
	}

	public void OnClickMoneySettlementLaw()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OnClickMoneySettlementLaw_Common();
	}
}
