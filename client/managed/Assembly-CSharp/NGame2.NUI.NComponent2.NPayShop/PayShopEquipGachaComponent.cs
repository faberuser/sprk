using System;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUtil.NPayShop;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaComponent : PayShopBaseComponent<NewEquipGachaData>, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopEquipGachaComponent";

	public static PrefabAuto<PayShopEquipGachaComponent> PrefabAuto = new PrefabAuto<PayShopEquipGachaComponent>("Prefab2/Component/PayShop2/PayShopEquipGachaComponent");

	public const string Small_PrefabPath = "Prefab2/Component/PayShop2/PayShopEquipGachaSmallComponent";

	public static PrefabAuto<PayShopEquipGachaComponent> Small_PrefabAuto = new PrefabAuto<PayShopEquipGachaComponent>("Prefab2/Component/PayShop2/PayShopEquipGachaSmallComponent");

	public UIWidget RootWidget;

	public UILabel Label_ProductName;

	public UILabel Label_Desc;

	public UILabel Label_Period;

	public UILabel Label_Count;

	public UISimpleGrid Grid_PurchaseInfo;

	public UIWidget Widget_SoldOut;

	public UISpriteSelector SpriteSelector_Flag;

	public UISpriteSelector GachaTypeSelector;

	public UIBundleLoader BundleLoader_ProductImage;

	public UISprite Sprite_Gacha10;

	public UIWidget PriceUIPosition;

	protected PayShopPriceComponent PriceComponent;

	public UILabel Label_Free;

	public UILabel Label_Ticket;

	public UIWidget OneTimeSaleIcon;

	public NewEquipGachaData EquipGachaData => _data;

	public NewPayShopProductInfo ProductInfo { get; private set; }

	protected override float UpdateInterval => 0.5f;

	public static PrefabAuto<PayShopEquipGachaComponent> GetPrefabAuto(int cardType)
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
	}

	protected override void Init(NewEquipGachaData data)
	{
		SetName(data);
		SetDesc(data);
		SetImage(data);
		SetSaleUI(data);
		InitGacha(data);
	}

	private void SetName(NewEquipGachaData data)
	{
		UIHelper.SetLabelText(text: (data.Type != EquipGachaType.Pickup) ? LocalizationManager.GetStringDirect(data.Name) : LocalizationManager.GetStringDirect(data.Name, data.PickupHeroCount), label: Label_ProductName);
	}

	private void SetDesc(NewEquipGachaData data)
	{
		string stringDirect = LocalizationManager.GetStringDirect(data.Desc);
		UIHelper.SetLabelText(Label_Desc, stringDirect);
	}

	private void SetPrice(NewEquipGachaData data)
	{
		bool flag = Checker.IsDiscount(data);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(data.Index);
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		ReleasePriceComponent();
		if (num)
		{
			UIHelper.EnableGameObject(Label_Free, enable: true);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
		}
		else if (freeGachaItemCount > 0 || data.TicketOnly)
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: true);
			string text = LocalizationManager.GetString(Gacha.FREE_GACHA_ITEM_COUNT, freeGachaItemCount);
			UIHelper.SetLabelText(Label_Ticket, text);
		}
		else if (data.OpenGem > 0 || data.OpenGold > 0)
		{
			bool num2 = data.OpenGem > 0;
			CurrencyType currency = ((!num2) ? CurrencyType.Gold : CurrencyType.Gem);
			int price;
			if (!num2)
			{
				price = data.OpenGold;
			}
			else
			{
				price = Calculator.GetGemApplyValidDiscounts(data);
				price = Calculator.GetStepUpDiscount(data, price);
			}
			if (PriceComponent == null)
			{
				PriceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceUIPosition.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(PriceComponent.transform);
				UIDepthHelper.RearrangeWidgetDepth(PriceComponent.gameObject, PriceUIPosition.depth);
			}
			if (flag)
			{
				PriceComponent.SetGamePrice(currency, price, data.OpenGem);
			}
			else
			{
				PriceComponent.SetGamePrice(currency, price);
			}
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(Label_Free, enable: true);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
		}
	}

	private void SetImage(NewEquipGachaData data)
	{
		if (!String_Extension_Method.IsNullOrEmpty(data.IconPath))
		{
			UIHelper.EnableGameObject(GachaTypeSelector, enable: false);
			UIHelper.EnableGameObject(BundleLoader_ProductImage, enable: true);
			UIHelper.SetBundleTexture(BundleLoader_ProductImage, data.IconPath);
		}
		else
		{
			UIHelper.EnableGameObject(GachaTypeSelector, enable: true);
			UIHelper.EnableGameObject(BundleLoader_ProductImage, enable: false);
			string gachaSpriteName = GetGachaSpriteName(data);
			GachaTypeSelector.Select(gachaSpriteName);
		}
		UIHelper.EnableGameObject(Sprite_Gacha10, data.Bid >= 10);
	}

	private static string GetGachaSpriteName(NewEquipGachaData data)
	{
		string text = data.Type.ToString();
		if (data.GachaSubType != EquipGachaSubType.None)
		{
			string text2 = data.GachaSubType.ToString();
			text = text + "_" + text2;
		}
		return text;
	}

	private void SetFlag(PayShopFlagType flag)
	{
		if (flag == PayShopFlagType.None)
		{
			UIHelper.EnableGameObject(SpriteSelector_Flag, enable: false);
			return;
		}
		UIHelper.EnableGameObject(SpriteSelector_Flag, enable: true);
		SpriteSelector_Flag.Select(flag.ToString());
	}

	private void SetSaleUI(NewEquipGachaData data)
	{
		bool flag = Checker.IsDiscount(data);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(data.Index);
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		if (num || freeGachaItemCount > 0)
		{
			UIHelper.EnableGameObject(OneTimeSaleIcon, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(OneTimeSaleIcon, flag);
		}
		PayShopFlagType flag2 = ((!num) ? (flag ? PayShopFlagType.Sale : PayShopFlagType.None) : PayShopFlagType.Free);
		SetFlag(flag2);
	}

	private void SetRemainCount(NewEquipGachaData data)
	{
		if (data.DailyMaxFree > 0)
		{
			int remainGachaChance = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.GetRemainGachaChance(EquipGachaData.Index);
			string text = LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_CHANCE, remainGachaChance, EquipGachaData.DailyMaxFree);
			UIHelper.EnableGameObject(Label_Count, enable: true);
			UIHelper.SetLabelText(Label_Count, text);
		}
		else if (data.StarterMileage > 0)
		{
			int remainLimitChance = GlobalAutoBehaviour<AccountManager>.instance.StarterGachaManager.GetRemainLimitChance(EquipGachaData.Index);
			string text2 = LocalizationManager.GetString(NShared.NLocalization.PayShop.PACKAGE_BUY_COUNT, remainLimitChance, data.StarterMileage);
			UIHelper.EnableGameObject(Label_Count, enable: true);
			UIHelper.SetLabelText(Label_Count, text2);
		}
		else
		{
			UIHelper.EnableGameObject(Label_Count, enable: false);
		}
	}

	private void InitGacha(NewEquipGachaData data)
	{
		UIHelper.EnableGameObject(Widget_SoldOut, enable: false);
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale: false);
		ProductInfo = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager.Get(data.PurchaseRestrictProductIndex);
		UpdateGachaUI(data, ProductInfo);
	}

	private void UpdateGachaUI(NewEquipGachaData data, NewPayShopProductInfo productInfo)
	{
		if (productInfo != null)
		{
			SetPeriodicalGacha(data, productInfo);
		}
		else
		{
			SetPrice(data);
			SetRemainCount(data);
			SetDuration(data);
		}
		UIHelper.RepositionGrid(Grid_PurchaseInfo);
	}

	private void SetDuration(NewEquipGachaData data)
	{
		EquipGachaInfo equipGachaInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipGachaManager.Get(data.Index);
		if (data.DailyMaxFree > 0)
		{
			bool flag = false;
			int remainGachaChance = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.GetRemainGachaChance(data.Index);
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.Free(EquipGachaData.Index);
			if (data.DailyMaxFree > 1)
			{
				flag = !flag2 && remainGachaChance > 0;
			}
			else if (data.DailyMaxFree == 1)
			{
				flag = !flag2;
			}
			UIHelper.EnableGameObject(Label_Period, flag);
			if (flag)
			{
				TimeSpan? remainFreeGachaCoolTimeSpan = GlobalAutoBehaviour<AccountManager>.instance.FreeEquipGachaManager.GetRemainFreeGachaCoolTimeSpan(EquipGachaData.Index);
				if (!remainFreeGachaCoolTimeSpan.HasValue)
				{
					UIHelper.EnableGameObject(Label_Period, enable: false);
					return;
				}
				string empty = string.Empty;
				UIHelper.SetLabelText(text: (remainFreeGachaCoolTimeSpan.Value.Hours <= 0) ? LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_COOLTIME, remainFreeGachaCoolTimeSpan.Value.Minutes, remainFreeGachaCoolTimeSpan.Value.Seconds) : LocalizationManager.GetString(Gacha.REMAINDAY_GACHA_COOLTIME_HOUR, remainFreeGachaCoolTimeSpan.Value.Hours, remainFreeGachaCoolTimeSpan.Value.Minutes), label: Label_Period);
			}
		}
		else if (data.Durational == 1)
		{
			if (equipGachaInfo != null)
			{
				UIHelper.EnableGameObject(Label_Period, enable: true);
				PayShopProductComponent.WriteProductPeriod(equipGachaInfo.BeginTime, equipGachaInfo.EndTime, Label_Period);
			}
			else
			{
				UIHelper.EnableGameObject(Label_Period, enable: false);
			}
		}
		else
		{
			UIHelper.EnableGameObject(Label_Period, enable: false);
		}
	}

	private void SetPeriodicalGacha(NewEquipGachaData data, NewPayShopProductInfo productInfo)
	{
		string resetDesc = PayShopUIHelper.GetResetDesc(productInfo);
		UIHelper.EnableGameObject(Label_Period, resetDesc != null);
		UIHelper.SetLabelText(Label_Period, resetDesc);
		PlayerProductPurchaseInfo playerProductPurchaseInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayerProductPurchaseManager.Get(productInfo.Index);
		int purchasableCount = productInfo.PurchasableCount;
		int num = purchasableCount - (playerProductPurchaseInfo?.PurchasedCount ?? 0);
		UIHelper.EnableGameObject(Label_Count, enable: true);
		UIHelper.SetLabelText(Label_Count, $"{num}/{purchasableCount}");
		ReleasePriceComponent();
		int freeGachaItemCount = HelperItem.GetFreeGachaItemCount(data.Index);
		bool flag = freeGachaItemCount > 0;
		bool flag2 = num > 0;
		if (flag)
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: true);
			string text = LocalizationManager.GetString(Gacha.FREE_GACHA_ITEM_COUNT, freeGachaItemCount);
			UIHelper.SetLabelText(Label_Ticket, text);
		}
		else
		{
			UIHelper.EnableGameObject(Label_Free, enable: false);
			UIHelper.EnableGameObject(Label_Ticket, enable: false);
			if (PriceComponent == null)
			{
				PriceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceUIPosition.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(PriceComponent.transform);
				UIDepthHelper.RearrangeWidgetDepth(PriceComponent.gameObject, PriceUIPosition.depth);
			}
			if (!flag2)
			{
				PriceComponent.NotBuyableOpen();
			}
			else
			{
				bool num2 = Checker.IsDiscount(data);
				bool num3 = data.OpenGem > 0;
				CurrencyType currency = ((!num3) ? CurrencyType.Gold : CurrencyType.Gem);
				int price = data.OpenGold;
				if (num3)
				{
					price = Calculator.GetGemApplyValidDiscounts(data);
					price = Calculator.GetStepUpDiscount(data, price);
				}
				if (num2)
				{
					PriceComponent.SetGamePrice(currency, price, data.OpenGem);
				}
				else
				{
					PriceComponent.SetGamePrice(currency, price);
				}
			}
		}
		UIHelper.SetWidgetGrayScale(RootWidget, !flag2 && !flag);
		UIHelper.EnableGameObject(Widget_SoldOut, !flag2 && !flag);
	}

	public override void ReleaseComponent()
	{
		ReleasePriceComponent();
	}

	private void ReleasePriceComponent()
	{
		FactoryBase.SafeReleaseComponent(ref PriceComponent);
	}

	public override void IntervalUpdateDerived()
	{
		UpdateGachaUI(_data, ProductInfo);
	}
}
