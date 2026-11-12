using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUtil.NCostume;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopCostumeComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public new interface IListener
	{
		void OnClickCostume(int costumeIndex);

		void OnClickOpenDiscountInfo(int productIndex);

		void OnClickBuyDiscountProduct(int productIndex, int discountPrice);
	}

	[Serializable]
	public class RootInfo
	{
		public CostumeState[] States;

		public Transform Root;
	}

	[Serializable]
	public class PriceInfo
	{
		public UIWidget Widget_PriceIcon;

		public UILabel Label_Price;

		[HideInInspector]
		public IconProxy IconProxy;

		public void SetPrice(CurrencyType currency, int price)
		{
			if (IconProxy == null)
			{
				IconProxy = IconProxy.Create(Widget_PriceIcon);
			}
			if (IconProxy != null)
			{
				IconProxy.SetupCurrencyNoBg(currency);
			}
			UIHelper.SetLabelText(Label_Price, Vespa_Int_Extension_Method.ToCommaString(price));
		}

		public void OnReleaseIconProxy()
		{
			if (IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref IconProxy);
				IconProxy = null;
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopCostumeComponent";

	public static PrefabAuto<PayShopCostumeComponent> PrefabAuto = new PrefabAuto<PayShopCostumeComponent>("Resources/Prefab2/Component/PayShop2/PayShopCostumeComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel Label_CostumeName;

	public RootInfo[] RootInfos;

	public UIWidget PriceRoot;

	public UISpriteSelector FlagSelector;

	public UISpriteSelector SaleSelector;

	[Header("Legend Costume")]
	public UIWidget Widget_NormalStep;

	public PriceInfo PriceInfo_CurrentPrice;

	public PriceInfo PriceInfo_NextPrice;

	public UIWidget Widget_MaxStep;

	public PriceInfo PriceInfo_FinalPrice;

	public UIClipSprite UIClipSprite_PreOrder;

	public UILabel Label_StepPercent;

	public UILabel Label_PreOrderCnt;

	public UIButton Button_Buy;

	public UILabel Label_Buy;

	public UIWidget Widget_TweenBuy;

	public UIWidget Widget_SoldOut;

	private PayShopPriceComponent _priceComponent;

	private IListener _listener;

	public CostumeData CostumeData { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	public void Open(CostumeData costumeData, IListener listener)
	{
		CostumeData = costumeData;
		_listener = listener;
		Refresh();
	}

	public void ReleaseComponent()
	{
		OnReleasePriceComponent();
	}

	private void OnReleasePriceComponent()
	{
		if (_priceComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _priceComponent);
		}
		if (PriceInfo_CurrentPrice != null)
		{
			PriceInfo_CurrentPrice.OnReleaseIconProxy();
		}
		if (PriceInfo_NextPrice != null)
		{
			PriceInfo_NextPrice.OnReleaseIconProxy();
		}
	}

	public void OnChangedPayShopItemEvent()
	{
		SetupCostumeInfos();
	}

	public void Refresh()
	{
		SetupCostumeInfos();
		SetupCommon();
	}

	private void SetupCommon()
	{
		if (CostumeData != null)
		{
			string portraitPath = CostumeData.PortraitPath;
			PortraitBundleTex.SetAssetPath(portraitPath);
			UIHelper.SetLabelText(Label_CostumeName, CostumeData.CostumeName);
			if (CostumeData.New != 0)
			{
				UIHelper.EnableGameObject(FlagSelector, enable: true);
				FlagSelector.Select(PayShopFlagType.New.ToString());
			}
			else
			{
				UIHelper.EnableGameObject(FlagSelector, enable: false);
			}
		}
	}

	private void SetupCostumeInfos()
	{
		int num = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(CostumeData.HeroIndex)?.CostumeIndex ?? 0;
		CostumeData equippedCostumeData = ((num != 0) ? GameRoot.Instance.CostumeDataContainer.GetData(num) : null);
		CostumeState costumeState = HeroCostume.GetCostumeState(equippedCostumeData, CostumeData);
		UIHelper.SetWidgetGrayScale(PortraitBundleTex.UiTex, grayScale: false);
		bool flag = costumeState == CostumeState.Equipped || costumeState == CostumeState.Owned;
		if (flag && CostumeData.IsCoupleCostume())
		{
			if (CostumeData.IsBonusCostume())
			{
				if (costumeState != CostumeState.Equipped)
				{
					CostumeData = GameRoot.Instance.CostumeDataContainer.GetData(CostumeData.MainIndex);
				}
			}
			else
			{
				int[] bonusCostumeIndices = CostumeData.BonusCostumeIndices;
				foreach (int num2 in bonusCostumeIndices)
				{
					if (num == num2)
					{
						CostumeData = GameRoot.Instance.CostumeDataContainer.GetData(num2);
					}
				}
			}
			costumeState = HeroCostume.GetCostumeState(equippedCostumeData, CostumeData);
		}
		for (int j = 0; j < RootInfos.Length; j++)
		{
			UIHelper.EnableGameObject(RootInfos[j].Root, Array.FindIndex(RootInfos[j].States, (CostumeState state) => state == costumeState) != -1);
		}
		int priceGem = HeroCostume.GetPriceGem(CostumeData);
		int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CostumeData);
		bool flag2 = priceDiscountPercentage > 0;
		bool num3 = flag2 && !flag;
		if (costumeState == CostumeState.Buyable || costumeState == CostumeState.NeedHero || costumeState == CostumeState.NeedSubStory)
		{
			UIHelper.EnableGameObject(PriceRoot, enable: true);
			if (_priceComponent == null)
			{
				_priceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceRoot.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_priceComponent.transform);
				UIDepthHelper.RearrangeWidgetDepth(_priceComponent.gameObject, PriceRoot.depth);
			}
			if (flag2)
			{
				_priceComponent.SetGamePrice(CurrencyType.Gem, priceGem, CostumeData.ReqBuyGem);
			}
			else
			{
				_priceComponent.SetGamePrice(CurrencyType.Gem, priceGem);
			}
		}
		else if (costumeState == CostumeState.LegendBuyable)
		{
			SetReservationInfos();
		}
		else
		{
			UIHelper.EnableGameObject(PriceRoot, enable: false);
		}
		if (num3)
		{
			UIHelper.EnableGameObject(SaleSelector, enable: true);
			SaleSelector.Select(priceDiscountPercentage.ToString());
		}
		else
		{
			UIHelper.EnableGameObject(SaleSelector, enable: false);
		}
	}

	public void SetReservationInfos()
	{
		UIHelper.EnableGameObject(PriceRoot, enable: false);
		int productIndex = CostumeData.ProductIndex;
		PayShopDiscountInfo payShopDiscountInfo = GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(productIndex);
		if (payShopDiscountInfo == null)
		{
			return;
		}
		PayShopDiscountData data = GameRoot.Instance.PayShopDiscountDataContainer.GetData(payShopDiscountInfo.ProductIndex);
		if (data == null)
		{
			return;
		}
		PayShopDiscountGroupData data2 = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(data.GroupIndex, payShopDiscountInfo.Step);
		PayShopDiscountGroupData data3 = GameRoot.Instance.PayShopDiscountGroupDataContainer.GetData(data.GroupIndex, payShopDiscountInfo.Step + 1);
		bool flag = data3 == null;
		UIHelper.EnableGameObject(Widget_MaxStep, flag);
		UIHelper.EnableGameObject(Widget_NormalStep, !flag);
		if (flag)
		{
			PriceInfo_FinalPrice.SetPrice(CurrencyType.Gem, payShopDiscountInfo.DiscountPrice);
			UIHelper.SetLabelText(Label_StepPercent, LocalizationManager.GetStringDirect("NEW_PAY_SHOP_CARD_TARGET_PERCENTAGE", 100));
			UIClipSprite_PreOrder.fillAmount = 1f;
		}
		else
		{
			PriceInfo_CurrentPrice.SetPrice(CurrencyType.Gem, payShopDiscountInfo.DiscountPrice);
			PriceInfo_NextPrice.SetPrice(CurrencyType.Gem, data3.Price);
			int num = data2?.TargetValue ?? 0;
			int num2 = data3.TargetValue - num;
			float num3 = (float)(payShopDiscountInfo.PurchasedCount - num) / (float)num2;
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			UIHelper.SetLabelText(Label_StepPercent, LocalizationManager.GetStringDirect("NEW_PAY_SHOP_CARD_TARGET_PERCENTAGE", (int)(num3 * 100f)));
			UIClipSprite_PreOrder.fillAmount = num3;
		}
		int serverLimitedCount = payShopDiscountInfo.ServerLimitedCount;
		bool flag2 = payShopDiscountInfo.PurchasedCount >= serverLimitedCount;
		int num4 = (flag2 ? serverLimitedCount : payShopDiscountInfo.PurchasedCount);
		UIHelper.SetLabelText(Label_PreOrderCnt, LocalizationManager.GetStringDirect("NEW_PAY_SHOP_CARD_ALL_PERSONNEL_3", num4, serverLimitedCount));
		bool flag3 = CheckLegendCostumeBuyable(productIndex, flag2);
		string key = ((flag3 || flag2) ? "NEW_PAY_SHOP_BUTTON_BID" : "COSTUME_LEGEND_PRE_ORDER_END");
		UIHelper.EnableButton(Button_Buy, flag3, grayscaleIfDisable: true);
		UIHelper.SetLabelText(Label_Buy, LocalizationManager.GetStringDirect(key));
		UIHelper.EnableGameObject(Widget_TweenBuy, flag3);
		UIHelper.SetWidgetGrayScale(PortraitBundleTex.UiTex, flag2);
		UIHelper.EnableGameObject(Widget_SoldOut, flag2);
	}

	private bool CheckLegendCostumeBuyable(int productIndex, bool isSoldOut)
	{
		if (isSoldOut)
		{
			return false;
		}
		if (productIndex == 0)
		{
			return false;
		}
		NewPayShopProductInfo newPayShopProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(productIndex);
		if (newPayShopProductInfo == null)
		{
			return false;
		}
		return PayShopHelper.IsBuyable(newPayShopProductInfo);
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickCostume(CostumeData.CostumeIndex);
		}
	}

	public void OnClickOpenDiscountInfo()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenDiscountInfo(CostumeData.ProductIndex);
		}
	}

	public void OnClickBuyDiscountProduct()
	{
		if (_listener != null)
		{
			PayShopDiscountInfo payShopDiscountInfo = GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Get(CostumeData.ProductIndex);
			if (payShopDiscountInfo != null && GlobalAutoBehaviour<DropBonusEventManager>.instance.GetPayshopDiscountEffectiveEvent(CostumeData.ProductIndex) != null)
			{
				_listener.OnClickBuyDiscountProduct(CostumeData.ProductIndex, payShopDiscountInfo.DiscountPrice);
			}
		}
	}
}
