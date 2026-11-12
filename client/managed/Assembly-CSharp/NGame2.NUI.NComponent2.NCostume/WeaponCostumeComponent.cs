using System;
using NGame2.NUI.NManager;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NCostume;

public class WeaponCostumeComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClick(WeaponCostumeComponent component);
	}

	[Serializable]
	public class RootInfo
	{
		public CostumeState State;

		public Transform Root;
	}

	public enum ComponentType
	{
		None,
		SelectThema,
		SelectUnique,
		SelectCostume,
		NoProduct
	}

	public struct OpenData
	{
		public static OpenData NullData = new OpenData
		{
			ComponentType = ComponentType.None,
			WeaponCostumeData = null
		};

		public ComponentType ComponentType;

		public WeaponCostumeData WeaponCostumeData;

		public static OpenData CreateSelectThema()
		{
			return new OpenData
			{
				ComponentType = ComponentType.SelectThema,
				WeaponCostumeData = null
			};
		}

		public static OpenData CreateSelectUnique()
		{
			return new OpenData
			{
				ComponentType = ComponentType.SelectUnique,
				WeaponCostumeData = null
			};
		}

		public static OpenData CreateSelectCostume(WeaponCostumeData costumeData)
		{
			if (costumeData == null)
			{
				VespaDebug.LogError("CreateSelectCostume Fail : costumeData null");
				return NullData;
			}
			return new OpenData
			{
				ComponentType = ComponentType.SelectCostume,
				WeaponCostumeData = costumeData
			};
		}

		public static OpenData CreatureNoProduct()
		{
			return new OpenData
			{
				ComponentType = ComponentType.NoProduct,
				WeaponCostumeData = null
			};
		}

		public bool IsValid()
		{
			if (!IsEqual(NullData))
			{
				return true;
			}
			return false;
		}

		public bool IsEqual(OpenData rhs)
		{
			if (ComponentType != rhs.ComponentType)
			{
				return false;
			}
			return Vespa_Common_Class_Extension_Method.EQUALS(WeaponCostumeData, rhs.WeaponCostumeData);
		}
	}

	public const string PrefabPath = "Prefab2/Component/Costume/WeaponCostumeComponent";

	public static PrefabAuto<WeaponCostumeComponent> PrefabAuto = new PrefabAuto<WeaponCostumeComponent>("Prefab2/Component/Costume/WeaponCostumeComponent");

	public const string UniqueTexturePath = "Customize/Product/Portrait/Customize_Weapon_Unique";

	public const string ThemaTexturePath = "Customize/Product/Portrait/Customize_Weapon_Thema";

	public Transform ComingSoon;

	public RootInfo[] RootInfos;

	public UILabel NameLabel;

	public UILabel ReqGemLabel;

	public UILabel DiscountReqGemLabel;

	public UIWidget WidgetNeedCostume;

	public UIWidget WidgetPrice;

	public UIWidget WidgetSaleTag;

	public CostumeSaleTagInfo[] SaleTagInfos;

	public UIBundleLoader CostumeBundleTex;

	public UISprite Sprite_Selected;

	public UISprite Sprite_New;

	protected IListener _listener;

	public OpenData CurrentComponentOpenData { get; private set; }

	public WeaponCostumeData CurrentComponentWeaponData => CurrentComponentOpenData.WeaponCostumeData;

	public bool Open(OpenData openData, IListener listener)
	{
		if (!openData.IsValid())
		{
			VespaDebug.LogError("openData.IsValid() == false");
			return false;
		}
		_listener = listener;
		CurrentComponentOpenData = openData;
		Init();
		return true;
	}

	private void Init()
	{
		InitCostumeInfo();
		InitCostumeImage();
		SetCostumePrice();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			UIHelper.EnableGameObject(RootInfos[i].Root, enable: false);
		}
		UIHelper.EnableGameObject(Sprite_New, CurrentComponentWeaponData != null && CurrentComponentWeaponData.IsNew);
		SetSelected(Selected: false);
	}

	public void SetCostumePrice()
	{
		if (CurrentComponentWeaponData == null)
		{
			return;
		}
		UIHelper.EnableGameObject(WidgetPrice, CurrentComponentWeaponData.ReqBuyGem > 0);
		UIHelper.EnableGameObject(WidgetNeedCostume, !__Extension_Method_ICollection__.IsNullOrEmpty(CurrentComponentWeaponData.NeedCostume));
		if (CurrentComponentWeaponData.ReqBuyGem != 0)
		{
			int priceGem = WeaponCostume.GetPriceGem(CurrentComponentWeaponData);
			long i_value = priceGem;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CurrentComponentWeaponData);
			for (int i = 0; i < SaleTagInfos.Length; i++)
			{
				UIHelper.EnableGameObject(SaleTagInfos[i].Sprite_Percentage, SaleTagInfos[i].DiscountPercentage == priceDiscountPercentage);
			}
			UIHelper.EnableGameObject(ReqGemLabel, priceDiscountPercentage == 0);
			UIHelper.EnableGameObject(DiscountReqGemLabel, priceDiscountPercentage > 0);
			if (priceDiscountPercentage == 0)
			{
				UIHelper.SetLabelText(ReqGemLabel, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
			if (priceDiscountPercentage > 0)
			{
				UIHelper.SetLabelText(DiscountReqGemLabel, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
		}
	}

	protected void InitCostumeInfo()
	{
		if (CurrentComponentOpenData.ComponentType == ComponentType.SelectUnique)
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_WEAPON_UNIQUE_NAME"));
		}
		else if (CurrentComponentOpenData.ComponentType == ComponentType.SelectThema)
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_WEAPON_THEME_NAME"));
		}
		if (CurrentComponentWeaponData != null)
		{
			UIHelper.SetLabelText(NameLabel, CurrentComponentWeaponData.Name);
			if (ClientValueManager.IsShowCostumeIndex)
			{
				UIHelper.SetLabelText(NameLabel, $"({CurrentComponentWeaponData.Index}){NameLabel.text}");
			}
		}
	}

	private void InitCostumeImage()
	{
		bool enable = CurrentComponentOpenData.ComponentType == ComponentType.NoProduct;
		bool enable2 = CurrentComponentOpenData.ComponentType != ComponentType.None && CurrentComponentOpenData.ComponentType != ComponentType.NoProduct;
		UIHelper.EnableGameObject(ComingSoon, enable);
		UIHelper.EnableGameObject(CostumeBundleTex, enable2);
		string text = string.Empty;
		if (CurrentComponentOpenData.ComponentType == ComponentType.SelectUnique)
		{
			text = "Customize/Product/Portrait/Customize_Weapon_Unique";
		}
		else if (CurrentComponentOpenData.ComponentType == ComponentType.SelectThema)
		{
			text = "Customize/Product/Portrait/Customize_Weapon_Thema";
		}
		else if (CurrentComponentOpenData.ComponentType == ComponentType.SelectCostume && CurrentComponentWeaponData != null)
		{
			text = CurrentComponentWeaponData.PortraitPath;
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.SetBundleTexture(CostumeBundleTex, text);
		}
	}

	public void SetState(CostumeState state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			bool flag = RootInfos[i].State == state;
			bool flag2 = state == CostumeState.Equipped || CurrentComponentOpenData.ComponentType == ComponentType.SelectCostume;
			UIHelper.EnableGameObject(RootInfos[i].Root, flag && flag2);
		}
		bool enable = state == CostumeState.Buyable && CurrentComponentWeaponData.ReqBuyGem > 0;
		UIHelper.EnableGameObject(WidgetSaleTag, enable);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_listener = null;
	}

	public void OnClick()
	{
		if (CurrentComponentOpenData.ComponentType != ComponentType.NoProduct && _listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void SetSelected(bool Selected)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Selected);
	}
}
