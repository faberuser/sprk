using NGame2.NPayShopItemEvent;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public sealed class PayShopHeroComponent : PayShopBaseComponent<CreatureData>, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopHeroComponent";

	public static PrefabAuto<PayShopHeroComponent> PrefabAuto = new PrefabAuto<PayShopHeroComponent>("Resources/Prefab2/Component/PayShop2/PayShopHeroComponent");

	public UIBundleLoader PortraitBundleTex;

	public UILabel Label_HeroName;

	public UIWidget WidgetPrice;

	public UILabel Label_Price;

	public UILabel LabelSalePrice;

	public UIButton Button_Buy;

	public UISpriteSelector SpriteSelectorClassTag;

	public UISpriteSelector SpriteSelectorStar;

	public UISpriteSelector SpriteSelectorDamageType;

	public Transform SaleRoot;

	public UISprite Sprite_New;

	private ShopSaleComponent _saleComponent;

	public CreatureData CreatureData
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

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ReleaseSaleComponent();
	}

	public override void Open(CreatureData creatureData, IListener listener)
	{
		base.Open(creatureData, listener);
	}

	protected override void Init(CreatureData data)
	{
		SetClassTag();
		if (data.OpenType == CreatureOpenType.Opened)
		{
			Setup();
		}
		UIHelper.EnableGameObject(Button_Buy, data.OpenType == CreatureOpenType.Opened);
		UIHelper.EnableGameObject(Sprite_New, data.OpenStatus == CreatureOpenStatusType.New);
	}

	public override void ReleaseComponent()
	{
		ReleaseSaleComponent();
	}

	private void SetClassTag()
	{
		if (SpriteSelectorClassTag != null)
		{
			SpriteSelectorClassTag.Select(CreatureData.TagType.ToString());
		}
	}

	private void SetStar(int star)
	{
		if (SpriteSelectorStar != null)
		{
			SpriteSelectorStar.SelectInt(star);
		}
	}

	private void SetDamageType(bool enable, SkillAttrType[] damageType = null)
	{
		if (SpriteSelectorDamageType == null)
		{
			return;
		}
		if (damageType == null)
		{
			UIHelper.EnableGameObject(SpriteSelectorDamageType, enable: false);
			return;
		}
		UIHelper.EnableGameObject(SpriteSelectorDamageType, enable);
		if (damageType.Contains(SkillAttrType.Physical))
		{
			SpriteSelectorDamageType.Select("Physical");
		}
		else if (damageType.Contains(SkillAttrType.Magical))
		{
			SpriteSelectorDamageType.Select("Magical");
		}
	}

	private void Setup()
	{
		string heroPortraitPath = UIHelper.GetHeroPortraitPath(CreatureData.CodeName, CreatureData.StartHeroStar);
		UIHelper.SetBundleTexture(PortraitBundleTex, heroPortraitPath);
		UIHelper.SetLabelText(Label_HeroName, CreatureData.Name);
		SetStar(CreatureData.StartHeroStar);
		SetDamageType(CreatureData.PlayableCharacter, CreatureData.AttrType);
		int num = 0;
		if (GlobalAutoBehaviour<PayShopItemEventManager>.isValidInstance)
		{
			num = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(CreatureData);
		}
		bool flag = num > 0;
		if (flag)
		{
			SetSale(num);
		}
		SetPrice(flag);
	}

	public void OnChangedPayShopItemEvent()
	{
		if (CreatureData.OpenType == CreatureOpenType.Opened)
		{
			int discountPercentage = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(CreatureData);
			bool flag = discountPercentage > 0;
			if (flag)
			{
				SetSale(discountPercentage);
			}
			else
			{
				ReleaseSaleComponent();
			}
			SetPrice(flag);
		}
	}

	private void SetPrice(bool isSale)
	{
		int price = GetPrice(CreatureData);
		long num = price;
		UIHelper.EnableGameObject(Label_Price, !isSale);
		UIHelper.EnableGameObject(LabelSalePrice, isSale);
		int num2 = 0;
		string text = ((num < num2) ? "(Error)" : Vespa_Int_Extension_Method.ToCommaString(num));
		if (isSale)
		{
			UIHelper.SetLabelText(Label_Price, text);
			UIHelper.SetLabelText(LabelSalePrice, text);
		}
		else
		{
			UIHelper.SetLabelText(Label_Price, text);
		}
	}

	private void SetSale(int discountPercentage)
	{
		if (!(SaleRoot == null))
		{
			if (_saleComponent == null)
			{
				_saleComponent = ShopSaleComponent.PrefabAuto.Factory.Create(SaleRoot);
			}
			if (_saleComponent != null)
			{
				_saleComponent.SetSaleSprite(discountPercentage);
			}
		}
	}

	private void ReleaseSaleComponent()
	{
		if (_saleComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _saleComponent);
			_saleComponent = null;
		}
	}

	public static int GetPrice(CreatureData creatureData)
	{
		int result = -1;
		if (creatureData == null)
		{
			return result;
		}
		CreatureStarPriceData data = GameRoot.Instance.CreatureStarPriceDataContainer.GetData(creatureData.OpenStatus, creatureData.StartHeroStar);
		if (data == null)
		{
			return result;
		}
		int discountPercentage = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(creatureData);
		return data.BuyGem * (100 - discountPercentage) / 100;
	}

	public static int GetMilieage(CreatureData creatureData)
	{
		int result = 0;
		if (creatureData == null)
		{
			return result;
		}
		CreatureStarPriceData data = GameRoot.Instance.CreatureStarPriceDataContainer.GetData(creatureData.OpenStatus, creatureData.StartHeroStar);
		if (data == null)
		{
			return result;
		}
		int discountPercentage = GlobalAutoBehaviour<PayShopItemEventManager>.instance.GetDiscountPercentage(creatureData);
		return data.Mileage * (100 - discountPercentage) / 100;
	}
}
