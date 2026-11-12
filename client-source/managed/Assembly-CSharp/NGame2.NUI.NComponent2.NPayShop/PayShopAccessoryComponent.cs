using NGame2.NUtil.NCostume;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopAccessoryComponent : PayShopBaseComponent<AccessoryCostumeData>
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopAccessoryComponent";

	public static PrefabAuto<PayShopAccessoryComponent> PrefabAuto = new PrefabAuto<PayShopAccessoryComponent>("Prefab2/Component/PayShop2/PayShopAccessoryComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel Label_CostumeName;

	public UILabel LabelCostumeDesc;

	public UISpriteSelector FlagSelector;

	public UISpriteSelector SaleSelector;

	public UIWidget PriceRoot;

	private PayShopPriceComponent _priceComponent;

	public AccessoryCostumeData AccessoryCostumeData
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

	public override void Open(AccessoryCostumeData data, IListener listener)
	{
		base.Open(data, listener);
	}

	protected override void Init(AccessoryCostumeData data)
	{
		Refresh();
	}

	public override void ReleaseComponent()
	{
		if (_priceComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _priceComponent);
		}
	}

	public override void Refresh()
	{
		SetupCostumeInfos();
		SetupCommon();
	}

	private void SetupCommon()
	{
		if (AccessoryCostumeData != null)
		{
			string productImagePath = AccessoryCostumeData.ProductImagePath;
			PortraitBundleTex.SetAssetPath(productImagePath);
			UIHelper.SetLabelText(Label_CostumeName, AccessoryCostumeData.Name);
			UIHelper.SetLabelText(LabelCostumeDesc, AccessoryCostumeData.ProductDesc);
			if (AccessoryCostumeData.New != 0)
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
		int priceGem = AccessoryCostume.GetPriceGem(AccessoryCostumeData);
		int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(AccessoryCostumeData);
		bool num = priceDiscountPercentage > 0;
		if (_priceComponent == null)
		{
			_priceComponent = PayShopPriceComponent.PrefabAuto.Factory.Create(PriceRoot.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_priceComponent.transform);
			UIDepthHelper.RearrangeWidgetDepth(_priceComponent.gameObject, PriceRoot.depth);
		}
		if (num)
		{
			_priceComponent.SetGamePrice(CurrencyType.Gem, priceGem, AccessoryCostumeData.ReqBuyGem);
			UIHelper.EnableGameObject(SaleSelector, enable: true);
			SaleSelector.Select(priceDiscountPercentage.ToString());
		}
		else
		{
			_priceComponent.SetGamePrice(CurrencyType.Gem, priceGem);
			UIHelper.EnableGameObject(SaleSelector, enable: false);
		}
	}

	public void OnChangedPayShopItemEvent()
	{
		SetupCostumeInfos();
	}
}
