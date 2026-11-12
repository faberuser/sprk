using System;
using NGame2.NUI.NManager;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NCostume;

public class HairCostumeComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClick(HairCostumeComponent component);
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
		SelectCostumeData,
		NoProduct
	}

	public struct OpenData
	{
		public static OpenData NullData = new OpenData
		{
			ComponentType = ComponentType.None,
			HairCostumeData = null
		};

		public ComponentType ComponentType;

		public HairCostumeData HairCostumeData;

		public static OpenData CreateSelectThema()
		{
			return new OpenData
			{
				ComponentType = ComponentType.SelectThema,
				HairCostumeData = null
			};
		}

		public static OpenData CreateSelectCostume(HairCostumeData costumeData)
		{
			return new OpenData
			{
				ComponentType = ComponentType.SelectCostumeData,
				HairCostumeData = costumeData
			};
		}

		public static OpenData CreatureNoProduct()
		{
			return new OpenData
			{
				ComponentType = ComponentType.NoProduct,
				HairCostumeData = null
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
			return Vespa_Common_Class_Extension_Method.EQUALS(HairCostumeData, rhs.HairCostumeData);
		}
	}

	public const string PrefabPath = "Prefab2/Component/Costume/HairCostumeComponent";

	public static PrefabAuto<HairCostumeComponent> PrefabAuto = new PrefabAuto<HairCostumeComponent>("Prefab2/Component/Costume/HairCostumeComponent");

	public const string ThemaTexturePath = "Customize/Product/Portrait/Customize_Hair_Thema";

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

	public HairCostumeData CurrentComponentHairData => CurrentComponentOpenData.HairCostumeData;

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
		UIHelper.EnableGameObject(Sprite_New, CurrentComponentHairData != null && CurrentComponentHairData.IsNew);
		SetSelected(Selected: false);
	}

	protected void InitCostumeInfo()
	{
		if (CurrentComponentOpenData.ComponentType != ComponentType.SelectCostumeData)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_HAIR_THEME_NAME");
			UIHelper.SetLabelText(NameLabel, stringDirect);
			return;
		}
		UIHelper.SetLabelText(NameLabel, CurrentComponentHairData.Name);
		UIHelper.SetLabelText(ReqGemLabel, CurrentComponentHairData.ReqBuyGem);
		if (ClientValueManager.IsShowCostumeIndex)
		{
			UIHelper.SetLabelText(NameLabel, $"({CurrentComponentHairData.Index}){NameLabel.text}");
		}
	}

	private void InitCostumeImage()
	{
		bool enable = CurrentComponentOpenData.ComponentType == ComponentType.NoProduct;
		bool enable2 = CurrentComponentOpenData.ComponentType == ComponentType.SelectCostumeData || CurrentComponentOpenData.ComponentType == ComponentType.SelectThema;
		UIHelper.EnableGameObject(ComingSoon, enable);
		UIHelper.EnableGameObject(CostumeBundleTex, enable2);
		string text = string.Empty;
		if (CurrentComponentOpenData.ComponentType == ComponentType.SelectThema)
		{
			text = "Customize/Product/Portrait/Customize_Hair_Thema";
		}
		else if (CurrentComponentOpenData.ComponentType == ComponentType.SelectCostumeData && CurrentComponentHairData != null)
		{
			text = CurrentComponentHairData.PortraitPath;
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.SetBundleTexture(CostumeBundleTex, text);
		}
	}

	public void SetCostumePrice()
	{
		if (CurrentComponentHairData == null)
		{
			return;
		}
		UIHelper.EnableGameObject(WidgetPrice, CurrentComponentHairData.ReqBuyGem > 0);
		UIHelper.EnableGameObject(WidgetNeedCostume, !__Extension_Method_ICollection__.IsNullOrEmpty(CurrentComponentHairData.NeedCostume));
		if (CurrentComponentHairData.ReqBuyGem != 0)
		{
			int priceGem = HairCostume.GetPriceGem(CurrentComponentHairData);
			long i_value = priceGem;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CurrentComponentHairData);
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

	public void SetState(CostumeState state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			bool flag = RootInfos[i].State == state;
			bool flag2 = state == CostumeState.Equipped || CurrentComponentOpenData.ComponentType == ComponentType.SelectCostumeData;
			UIHelper.EnableGameObject(RootInfos[i].Root, flag && flag2);
		}
		bool enable = state == CostumeState.Buyable && CurrentComponentHairData.ReqBuyGem > 0;
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
