using System;
using System.Collections.Generic;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NHelper;
using NGame2.NUI.NWindow.NPayShop;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopProductComponent : PayShopBaseComponent<NewPayShopProductInfo>, NVespa.NPrefab2.InitOnCreate
{
	public new interface IListener : PayShopBaseComponent.IListener
	{
		void OnClickProduct(int originalIndex, int comboIndex);

		void OnChangeComboProduct(int productIndex, int step);
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopProductComponent";

	public static PrefabAuto<PayShopProductComponent> PrefabAuto = new PrefabAuto<PayShopProductComponent>("Prefab2/Component/PayShop2/PayShopProductComponent");

	public const string Small_PrefabPath = "Prefab2/Component/PayShop2/PayShopProductSmallComponent";

	public static PrefabAuto<PayShopProductComponent> Small_PrefabAuto = new PrefabAuto<PayShopProductComponent>("Prefab2/Component/PayShop2/PayShopProductSmallComponent");

	public UIWidget RootWidget;

	public UILabel Label_ProductName;

	public UILabel Label_PurchasePeriod;

	public UILabel Label_PurchasableCount;

	public UISimpleGrid Grid_PurchaseInfo;

	public UISpriteSelector SpriteSelector_Flag;

	public UIBundleLoader BundleLoader_ProductImage;

	public UIBundleLoader BundleLoader_ProductLargeImage;

	public UIWidget Widget_SoldOut;

	public UILabelAligner SpecialCurrencyAligner;

	public UIWidget Widget_StepProductRoot;

	public UILabel Label_StepCount;

	public UIWidget PriceUIPosition;

	protected PayShopPriceComponent PriceComponent;

	public UIButton Button_Buy;

	public Transform BonusInfoRoot;

	public UILabel LabelBonusDesc;

	public UIBundleLoader BundleBonusIcon;

	public UILabel LabelProductIndex;

	public int CardPlateType = 2;

	private List<PayShopSpecialCurrencyComponent> _specialCurrencyComponents = new List<PayShopSpecialCurrencyComponent>();

	private float PrevUpdateTime;

	private new IListener _listener;

	public NewPayShopProductInfo OriginalInfo { get; private set; }

	public NewPayShopProductInfo ProductInfo
	{
		get
		{
			return _data;
		}
		private set
		{
			_data = value;
		}
	}

	public SkuInfo SkuInfo { get; private set; }

	public PlayerProductPurchaseInfo PurchaseInfo { get; private set; }

	public bool IsSoldOut { get; private set; }

	public static PrefabAuto<PayShopProductComponent> GetPrefabAuto(int cardType)
	{
		return cardType switch
		{
			1 => PrefabAuto, 
			2 => Small_PrefabAuto, 
			_ => Small_PrefabAuto, 
		};
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ProductInfo = null;
		_listener = null;
		ReleaseComponent();
	}

	public override void ReleaseComponent()
	{
		FactoryBase.SafeReleaseComponent(ref PriceComponent);
		ReleaseSpecialCurrencyComponents();
	}

	public override void Open(NewPayShopProductInfo productInfo, PayShopBaseComponent.IListener listener)
	{
		_listener = listener as IListener;
		base.Open(productInfo, listener);
	}

	public void ChangeProductInfo(NewPayShopProductInfo productInfo)
	{
		ReleaseComponent();
		base.Open(productInfo, _listener);
	}

	private void SetInfo(NewPayShopProductInfo productInfo)
	{
		PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		string inAppPurchaseId = productInfo.InAppPurchaseId;
		SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(inAppPurchaseId, productInfo);
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
		}
		OriginalInfo = productInfo;
		PurchaseInfo = purchaseInfo;
		SkuInfo = skuInfo;
	}

	protected override void Init(NewPayShopProductInfo productInfo)
	{
		SetInfo(productInfo);
		Init(productInfo, PurchaseInfo, SkuInfo);
	}

	public void Init(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, SkuInfo skuInfo)
	{
		SetBuyableState(isBuyable: true);
		string stringDirect = LocalizationManager.GetStringDirect(productInfo.CardTitle);
		SetCardName(stringDirect);
		SetPurchaseInfo(productInfo, purchaseInfo);
		if (productInfo.PurchasableCount > 0 && productInfo.PurchasableCount - (purchaseInfo?.PurchasedCount ?? 0) <= 0)
		{
			SetBuyableState(isBuyable: false);
		}
		SetProductTexture(productInfo.IconPath, productInfo.LongIconPath);
		SetFlag(productInfo);
		SetPrice(productInfo, purchaseInfo, skuInfo);
		SetStepProductUI(productInfo);
		SetSpecialCurrency(productInfo);
		SetBonusInfo(productInfo);
		SetProductIndex(productInfo);
		SprkShopSearch.Decorate(this, productInfo);
	}

	private void OnEnable()
	{
		UIHelper.RepositionGrid(Grid_PurchaseInfo);
	}

	private void Update()
	{
		ProductUpdate();
	}

	protected void ProductUpdate()
	{
		if (Time.time - PrevUpdateTime >= 1f)
		{
			PrevUpdateTime = Time.time;
			IntervalUpdate();
		}
	}

	protected virtual void IntervalUpdate()
	{
		if (ProductInfo != null && ProductInfo.IsPurchaseMarketing)
		{
			UpdatePurchaseMarketing(ProductInfo);
		}
	}

	public override void OnClickComponent()
	{
		if (OriginalInfo != null && ProductInfo != null && OriginalInfo.Index != ProductInfo.Index)
		{
			if (_listener != null)
			{
				_listener.OnClickProduct(OriginalInfo.Index, ProductInfo.Index);
			}
		}
		else
		{
			base.OnClickComponent();
		}
	}

	protected void SetCardName(string name)
	{
		UIHelper.SetLabelText(Label_ProductName, name);
	}

	protected void SetPurchaseInfo(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		if (CardPlateType == 1)
		{
			SetPurchseableCount(productInfo, purchaseInfo);
			SetPurchasePeriod(productInfo);
		}
		else
		{
			SetPurchsePeriodCount(productInfo, purchaseInfo);
		}
		UIHelper.RepositionGrid(Grid_PurchaseInfo);
	}

	protected void SetPurchseableCount(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		PayShopUIHelper.SetPurchseableCount(productInfo, purchaseInfo, Label_PurchasableCount);
	}

	protected void SetPurchasePeriod(NewPayShopProductInfo productInfo)
	{
		PayShopUIHelper.SetPurchasePeriod(productInfo, Label_PurchasePeriod);
	}

	public static bool WriteProductPeriod(string beginTime, string endTime, UILabel label)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(beginTime) && !String_Extension_Method.IsNullOrEmpty(endTime);
		UIHelper.EnableGameObject(label, flag);
		if (flag)
		{
			DateTime clientTime = UIHelper.GetClientTime(beginTime);
			DateTime clientTime2 = UIHelper.GetClientTime(endTime);
			string text = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD_DATE, clientTime.Month, clientTime.Day);
			string text2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD_DATE, clientTime2.Month, clientTime2.Day);
			string text3 = LocalizationManager.GetString(NShared.NLocalization.PayShop.PRODUCT_SELL_PERIOD, text, text2);
			UIHelper.SetLabelText(label, text3);
		}
		return flag;
	}

	protected void SetProductTexture(string path, string largePath = null)
	{
		if (BundleLoader_ProductLargeImage == null)
		{
			UIHelper.SetBundleTexture(BundleLoader_ProductImage, path);
		}
		else if (!String_Extension_Method.IsNullOrEmpty(largePath))
		{
			UIHelper.SetBundleTexture(BundleLoader_ProductLargeImage, largePath);
			UIHelper.EnableGameObject(BundleLoader_ProductImage, enable: false);
			UIHelper.EnableGameObject(BundleLoader_ProductLargeImage, enable: true);
		}
		else
		{
			UIHelper.SetBundleTexture(BundleLoader_ProductImage, path);
			UIHelper.EnableGameObject(BundleLoader_ProductImage, enable: true);
			UIHelper.EnableGameObject(BundleLoader_ProductLargeImage, enable: false);
		}
	}

	protected void SetPrice(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, SkuInfo skuInfo)
	{
		if (PriceComponent == null)
		{
			PriceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceUIPosition.transform);
		}
		UIDepthHelper.RearrangeWidgetDepth(PriceComponent.gameObject, PriceUIPosition.depth);
		PriceComponent.Open(productInfo, purchaseInfo, skuInfo);
		bool enable = productInfo == OriginalInfo && PriceComponent.IsBuyable;
		UIHelper.EnableButton(Button_Buy, enable, grayscaleIfDisable: true);
	}

	public void SetPriceWidgetVisible(bool Visible)
	{
		UIHelper.EnableGameObject(PriceComponent, Visible);
	}

	protected bool IsEvent(NewPayShopProductInfo productInfo)
	{
		bool result = false;
		if (productInfo.ProductType == ProductType.Gem)
		{
			DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.PaybackGem, string.Empty);
			result = effectiveEvent != null && effectiveEvent.ItemIndices != null && Array.FindIndex(effectiveEvent.ItemIndices, (int itemIndex) => itemIndex == productInfo.Index) != -1;
		}
		return result;
	}

	protected void UpdatePurchaseMarketing(NewPayShopProductInfo productInfo)
	{
		SetPurchasePeriod(productInfo);
	}

	public void SetBuyableState(bool isBuyable)
	{
		SetGrayScale(!isBuyable);
		SetNotBuyableSprite(!isBuyable);
	}

	protected void SetGrayScale(bool isGrayScale)
	{
		UIHelper.SetWidgetGrayScale(RootWidget, isGrayScale);
	}

	protected void SetNotBuyableSprite(bool isShow)
	{
		IsSoldOut = isShow;
		UIHelper.EnableGameObject(Widget_SoldOut, isShow);
	}

	public void ShowChangeProductButton(bool isShow)
	{
		UIHelper.EnableGameObject(Widget_StepProductRoot, isShow);
	}

	protected void SetStepProductUI(NewPayShopProductInfo productInfo)
	{
		bool flag = __Extension_Method_ICollection__.COUNT(productInfo.StepProductGroup) > 0;
		ShowChangeProductButton(flag);
		if (!flag)
		{
			return;
		}
		int valueInt = ArgParser.GetValueInt(productInfo.StepProductGroup, 0);
		int valueInt2 = ArgParser.GetValueInt(productInfo.StepProductGroup, 1);
		NewPayShopProductInfo[] stepProducts = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProducts(productInfo.Category, valueInt);
		if (__Extension_Method_ICollection__.COUNT(stepProducts) == 0)
		{
			return;
		}
		int num = 0;
		NewPayShopProductInfo[] array = stepProducts;
		for (int i = 0; i < array.Length; i++)
		{
			int valueInt3 = ArgParser.GetValueInt(array[i].StepProductGroup, 1);
			if (num < valueInt3)
			{
				num = valueInt3;
			}
		}
		UIHelper.SetLabelText(Label_StepCount, valueInt2 + "/" + num);
	}

	private void ReleaseSpecialCurrencyComponents()
	{
		FactoryBase.ReleaseAndClearList(_specialCurrencyComponents);
	}

	private Dictionary<CurrencyType, int> GetPayShopPoint(NewPayShopProductInfo productInfo)
	{
		if (productInfo == null)
		{
			return null;
		}
		if (productInfo.ProductType != ProductType.Package)
		{
			return null;
		}
		ItemData itemData = null;
		int num = -1;
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			num = ((__Extension_Method_ICollection__.COUNT(productInfo.ItemInfos) > 0) ? productInfo.ItemInfos[0].ItemIndex : (-1));
		}
		else
		{
			string code = ((__Extension_Method_ICollection__.COUNT(productInfo.UseItemCode) > 0) ? productInfo.UseItemCode[0] : string.Empty);
			itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (itemData == null)
			{
				return null;
			}
			num = itemData.Index;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num);
		if (data == null)
		{
			return null;
		}
		List<RewardElementData> list = new List<RewardElementData>();
		PackageItemHelper.GetRewardElementList(data, list, null);
		Dictionary<CurrencyType, int> dictionary = new Dictionary<CurrencyType, int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			RewardElementData rewardElementData = list[i];
			CurrencyType currencyType = rewardElementData.GetCurrencyType();
			if (currencyType == CurrencyType.Mileage || currencyType == CurrencyType.LuaPoint || currencyType == CurrencyType.ShopEventPoint || currencyType == CurrencyType.LimitedShopEventPoint)
			{
				if (!dictionary.ContainsKey(currencyType))
				{
					dictionary.Add(currencyType, 0);
				}
				dictionary[currencyType] += rewardElementData.ItemMin;
			}
		}
		return dictionary;
	}

	public void SetSpecialCurrency(NewPayShopProductInfo productInfo)
	{
		if (!(SpecialCurrencyAligner == null))
		{
			Dictionary<CurrencyType, int> payShopPoint = GetPayShopPoint(productInfo);
			int num = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(payShopPoint) && payShopPoint.ContainsKey(CurrencyType.Mileage))
			{
				num += payShopPoint[CurrencyType.Mileage];
			}
			if (productInfo.Mileage > 0)
			{
				num += productInfo.Mileage;
			}
			if (num > 0)
			{
				AddSpecialCurrencyComponent(CurrencyType.Mileage, num);
			}
			int num2 = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(payShopPoint) && payShopPoint.ContainsKey(CurrencyType.LuaPoint))
			{
				num2 += payShopPoint[CurrencyType.LuaPoint];
			}
			if (productInfo.LuaPoint > 0)
			{
				num2 += productInfo.LuaPoint;
			}
			if (num2 > 0)
			{
				AddSpecialCurrencyComponent(CurrencyType.LuaPoint, num2);
			}
			int num3 = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(payShopPoint) && payShopPoint.ContainsKey(CurrencyType.ShopEventPoint))
			{
				num3 += payShopPoint[CurrencyType.ShopEventPoint];
			}
			if (num3 > 0)
			{
				AddSpecialCurrencyComponent(CurrencyType.ShopEventPoint, num3);
			}
			int num4 = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(payShopPoint) && payShopPoint.ContainsKey(CurrencyType.LimitedShopEventPoint))
			{
				num4 += payShopPoint[CurrencyType.LimitedShopEventPoint];
			}
			if (productInfo.LimitedShopEventPoint > 0)
			{
				num4 += productInfo.LimitedShopEventPoint;
			}
			if (num4 > 0)
			{
				AddSpecialCurrencyComponent(CurrencyType.LimitedShopEventPoint, num4);
			}
			SpecialCurrencyAligner.CollectWidget();
			SpecialCurrencyAligner.Align();
		}
	}

	private void AddSpecialCurrencyComponent(CurrencyType type, int point)
	{
		if (!(SpecialCurrencyAligner == null))
		{
			PayShopSpecialCurrencyComponent payShopSpecialCurrencyComponent = PayShopSpecialCurrencyComponent.PrefabAuto.Factory.Create(SpecialCurrencyAligner.transform);
			payShopSpecialCurrencyComponent.Open(type, point);
			_specialCurrencyComponents.Add(payShopSpecialCurrencyComponent);
		}
	}

	protected void SetPurchsePeriodCount(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo)
	{
		if (!PayShopUIHelper.SetPurchasePeriod(productInfo, Label_PurchasableCount))
		{
			PayShopUIHelper.SetPurchseableCount(productInfo, purchaseInfo, Label_PurchasableCount);
		}
	}

	public void SetBonusInfo(NewPayShopProductInfo productInfo)
	{
		if (!(BonusInfoRoot == null))
		{
			string[] bonusProductInfos = productInfo.BonusProductInfos;
			bool flag = __Extension_Method_ICollection__.COUNT(bonusProductInfos) >= 2;
			UIHelper.EnableGameObject(BonusInfoRoot, flag);
			if (flag)
			{
				string valueString = ArgParser.GetValueString(bonusProductInfos, 0);
				string stringDirect = LocalizationManager.GetStringDirect(ArgParser.GetValueString(bonusProductInfos, 1));
				UIHelper.SetBundleTexture(BundleBonusIcon, valueString);
				UIHelper.SetLabelText(LabelBonusDesc, stringDirect);
			}
		}
	}

	public void SetProductIndex(NewPayShopProductInfo productInfo)
	{
		bool flag = false;
		UIHelper.EnableGameObject(LabelProductIndex, flag);
		if (flag && productInfo != null)
		{
			UIHelper.SetLabelText(LabelProductIndex, productInfo.Index);
		}
	}

	public void OnClickChangeProduct()
	{
		if (ProductInfo != null && __Extension_Method_ICollection__.COUNT(ProductInfo.StepProductGroup) != 0)
		{
			int nextStep = ArgParser.GetValueInt(ProductInfo.StepProductGroup, 1) + 1;
			SetNextStepInfo(ref nextStep);
			if (_listener != null)
			{
				_listener.OnChangeComboProduct(OriginalInfo.Index, nextStep);
			}
		}
	}

	public void SetNextStepInfo(ref int nextStep)
	{
		if (ProductInfo == null || __Extension_Method_ICollection__.COUNT(ProductInfo.StepProductGroup) == 0)
		{
			return;
		}
		int valueInt = ArgParser.GetValueInt(ProductInfo.StepProductGroup, 0);
		NewPayShopProductInfo[] stepProducts = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.GetStepProducts(ProductInfo.Category, valueInt);
		if (__Extension_Method_ICollection__.COUNT(stepProducts) != 0)
		{
			NewPayShopProductInfo stepProduct = GetStepProduct(stepProducts, nextStep);
			if (stepProduct == null)
			{
				nextStep = 1;
				stepProduct = GetStepProduct(stepProducts, nextStep);
			}
			PlayerProductPurchaseInfo purchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(stepProduct.Index);
			string inAppPurchaseId = stepProduct.InAppPurchaseId;
			SkuInfo skuInfo = SingletonAuto<IAPWrapper>.instance.GetSkuInfo(inAppPurchaseId);
			ProductInfo = stepProduct;
			PurchaseInfo = purchaseInfo;
			SkuInfo = skuInfo;
			ReleaseComponent();
			Init(stepProduct, purchaseInfo, skuInfo);
		}
	}

	public NewPayShopProductInfo GetStepProduct(NewPayShopProductInfo[] products, int step)
	{
		if (__Extension_Method_ICollection__.COUNT(products) == 0)
		{
			return null;
		}
		return Array.Find(products, (NewPayShopProductInfo p) => ArgParser.GetValueInt(p.StepProductGroup, 1) == step);
	}

	private void SetFlag(NewPayShopProductInfo product)
	{
		if (!(SpriteSelector_Flag == null))
		{
			UIHelper.EnableGameObject(SpriteSelector_Flag, enable: true);
			if (product.Flag == PayShopFlagType.None)
			{
				UIHelper.EnableGameObject(SpriteSelector_Flag, enable: false);
			}
			else
			{
				SpriteSelector_Flag.Select(product.Flag.ToString());
			}
		}
	}
}
