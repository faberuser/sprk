using NGame2.IAP;
using NGame2.NGuild;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopPriceComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopPriceComponent";

	public static PrefabAuto<PayShopPriceComponent> PrefabAuto = new PrefabAuto<PayShopPriceComponent>("Resources/Prefab2/Component/PayShop2/PayShopPriceComponent");

	public UIWidget IconRoot;

	public UIWidget IconProxyRoot;

	public UIWidget PriceRoot;

	public UILabelAligner Aligner;

	public UILabel PriceLabel;

	public UILabel OriginalPriceLabel;

	public UILabel CashCurrencyLabel;

	public UILabel NotBuyableLabel;

	public UIWidget FreePriceRoot;

	public Color PriceColorDefault;

	public Color PriceColorOnSale;

	private IconProxy IconProxy;

	public bool IsBuyable { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		UIHelper.EnableGameObject(PriceRoot, enable: false);
		UIHelper.EnableGameObject(NotBuyableLabel, enable: false);
		UIHelper.EnableGameObject(FreePriceRoot, enable: false);
		IsBuyable = true;
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
		Init();
	}

	public void Open(NewPayShopProductInfo productInfo, PlayerProductPurchaseInfo purchaseInfo, SkuInfo skuInfo)
	{
		Init();
		if (PayShopHelper.IsBuyable(productInfo))
		{
			if (!String_Extension_Method.IsNullOrEmpty(productInfo.InAppPurchaseId))
			{
				SetCashPrice(skuInfo);
			}
			else
			{
				SetGamePrice(productInfo.PriceType, (int)productInfo.Price);
			}
		}
		else
		{
			SetBuyable(isBuyable: false);
		}
	}

	public void NotBuyableOpen()
	{
		Init();
		SetBuyable(isBuyable: false);
	}

	private static bool PurchasableGuildNameChange(ItemData itemData)
	{
		bool result = false;
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L && GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && myMemberInfo.Rank == 1)
			{
				result = true;
			}
		}
		return result;
	}

	protected static bool SetPriceForItem(NewPayShopProductInfo productInfo)
	{
		bool result = true;
		if (productInfo.Category == NewPayShopProductCategoryType.RightAway.ToString())
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(productInfo.ItemInfos); i++)
			{
				PayShopProductItemInfo payShopProductItemInfo = productInfo.ItemInfos[i];
				if (payShopProductItemInfo != null)
				{
					ItemData data = GameRoot.Instance.ItemDataContainer.GetData(payShopProductItemInfo.ItemIndex);
					if (data != null && data.Type == ItemType.GuildNameChange)
					{
						result = PurchasableGuildNameChange(data);
						break;
					}
				}
			}
		}
		else
		{
			string[] useItemCode = productInfo.UseItemCode;
			foreach (string code in useItemCode)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
				if (dataByCode != null && dataByCode.Type == ItemType.GuildNameChange)
				{
					result = PurchasableGuildNameChange(dataByCode);
					break;
				}
			}
		}
		return result;
	}

	public void Init()
	{
		UIHelper.SetLabelColor(PriceLabel, PriceColorDefault);
		UIHelper.EnableGameObject(PriceRoot, enable: false);
		UIHelper.EnableGameObject(NotBuyableLabel, enable: false);
		UIHelper.EnableGameObject(FreePriceRoot, enable: false);
		IsBuyable = true;
	}

	public void SetCashPrice(SkuInfo skuInfo)
	{
		SetBuyable(isBuyable: false);
	}

	public void SetGamePrice(CurrencyType currency, int price, int originalPrice = 0)
	{
		if (currency == CurrencyType.None || price == 0)
		{
			SetFree(isFree: true);
			return;
		}
		UIHelper.EnableGameObject(PriceRoot, enable: true);
		UIHelper.EnableGameObject(CashCurrencyLabel, enable: false);
		UIHelper.EnableGameObject(IconRoot.gameObject, enable: true);
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconProxyRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(currency);
		}
		SetPrice(price, originalPrice);
		UpdatePriceAligner();
	}

	private void SetPrice(int price, int originalPrice = 0)
	{
		UIHelper.SetLabelText(PriceLabel, Vespa_Int_Extension_Method.ToCommaString(price));
		UIHelper.EnableGameObject(OriginalPriceLabel, originalPrice > 0);
		if (originalPrice > 0)
		{
			UIHelper.SetLabelColor(PriceLabel, PriceColorOnSale);
			UIHelper.SetLabelText(OriginalPriceLabel, Vespa_Int_Extension_Method.ToCommaString(originalPrice));
		}
	}

	public void SetBuyable(bool isBuyable)
	{
		IsBuyable = isBuyable;
		UIHelper.EnableGameObject(NotBuyableLabel, !isBuyable);
	}

	public void SetFree(bool isFree)
	{
		UIHelper.EnableGameObject(FreePriceRoot, isFree);
	}

	public void UpdatePriceAligner()
	{
		if (Aligner != null)
		{
			Aligner.Align();
		}
	}
}
