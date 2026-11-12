using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NCostume;

public class AccessoryCostumeComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(AccessoryCostumeComponent component);
	}

	public enum ComponentType
	{
		None,
		SelectUnset,
		SelectCostume,
		NoProduct
	}

	public struct OpenData
	{
		public static OpenData NullData = new OpenData
		{
			ComponentType = ComponentType.None,
			AccessoryPartType = AccessoryPartType.None,
			AccessoryCostumeData = null
		};

		public ComponentType ComponentType;

		public AccessoryPartType AccessoryPartType;

		public AccessoryCostumeData AccessoryCostumeData;

		public static OpenData CreateUnsetPart(AccessoryPartType partType)
		{
			if (partType == AccessoryPartType.None)
			{
				return NullData;
			}
			return new OpenData
			{
				ComponentType = ComponentType.SelectUnset,
				AccessoryPartType = partType,
				AccessoryCostumeData = null
			};
		}

		public static OpenData CreateCostumeType(AccessoryCostumeData costumeData)
		{
			if (costumeData == null)
			{
				return NullData;
			}
			return new OpenData
			{
				ComponentType = ComponentType.SelectCostume,
				AccessoryPartType = costumeData.PartType,
				AccessoryCostumeData = costumeData
			};
		}

		public static OpenData CreateNoProduct()
		{
			return new OpenData
			{
				ComponentType = ComponentType.NoProduct,
				AccessoryPartType = AccessoryPartType.None,
				AccessoryCostumeData = null
			};
		}

		public bool IsValid()
		{
			return !IsEqual(NullData);
		}

		public bool IsEqual(OpenData rhs)
		{
			if (ComponentType != rhs.ComponentType)
			{
				return false;
			}
			if (AccessoryPartType != rhs.AccessoryPartType)
			{
				return false;
			}
			return Vespa_Common_Class_Extension_Method.EQUALS(AccessoryCostumeData, rhs.AccessoryCostumeData);
		}
	}

	[Serializable]
	public class RootInfo
	{
		public CostumeState State;

		public Transform Root;
	}

	public const string PrefabPath = "Prefab2/Component/Costume/AccessoryCostumeComponent";

	public static PrefabAuto<AccessoryCostumeComponent> PrefabAuto = new PrefabAuto<AccessoryCostumeComponent>("Prefab2/Component/Costume/AccessoryCostumeComponent");

	public const string EmptyTexturePath = "Customize/Product/Portrait/Customize_Empty_Slot";

	public RootInfo[] RootInfos;

	public UILabel NameLabel;

	public UILabel DescLabel;

	public UILabel ReqGemLabel;

	public UILabel DiscountReqGemLabel;

	public Transform TransformComingSoon;

	public UIWidget WidgetSaleTag;

	public CostumeSaleTagInfo[] SaleTagInfos;

	public UIBundleLoader CostumeBundleTex;

	public UISprite Sprite_Selected;

	public UISprite Sprite_New;

	private Listener _listener;

	public OpenData ComponentOpenData { get; private set; }

	public bool Open(OpenData openData, Listener listener)
	{
		ComponentOpenData = openData;
		_listener = listener;
		InitCostumeInfo();
		InitCostumeImage();
		SetCostumePrice();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			UIHelper.EnableGameObject(RootInfos[i].Root, enable: false);
		}
		UIHelper.EnableGameObject(Sprite_New, openData.AccessoryCostumeData != null && openData.AccessoryCostumeData.New == 1);
		SetSelected(Selected: false);
		return true;
	}

	public void InitCostumeInfo()
	{
		if (ComponentOpenData.ComponentType == ComponentType.SelectUnset)
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NO_SELECT_ACCESSORY"));
		}
		if (ComponentOpenData.AccessoryCostumeData != null)
		{
			UIHelper.SetLabelText(NameLabel, ComponentOpenData.AccessoryCostumeData.Name);
			UIHelper.SetLabelText(DescLabel, ComponentOpenData.AccessoryCostumeData.ProductDesc);
			if (ClientValueManager.IsShowCostumeIndex)
			{
				UIHelper.SetLabelText(NameLabel, $"({ComponentOpenData.AccessoryCostumeData.Index}){NameLabel.text}");
			}
		}
	}

	public void SetState(CostumeState state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			bool flag = RootInfos[i].State == state;
			bool flag2 = state == CostumeState.Equipped || ComponentOpenData.ComponentType == ComponentType.SelectCostume;
			UIHelper.EnableGameObject(RootInfos[i].Root, flag && flag2);
		}
		UIHelper.EnableGameObject(WidgetSaleTag, state == CostumeState.Buyable);
	}

	public void SetCostumePrice()
	{
		if (ComponentOpenData.AccessoryCostumeData != null)
		{
			AccessoryCostumeData accessoryCostumeData = ComponentOpenData.AccessoryCostumeData;
			int priceGem = AccessoryCostume.GetPriceGem(accessoryCostumeData);
			long i_value = priceGem;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(accessoryCostumeData);
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

	private void InitCostumeImage()
	{
		UIHelper.EnableGameObject(TransformComingSoon, ComponentOpenData.ComponentType == ComponentType.NoProduct);
		UIHelper.EnableGameObject(CostumeBundleTex, ComponentOpenData.ComponentType == ComponentType.SelectCostume || ComponentOpenData.ComponentType == ComponentType.SelectUnset);
		string text = string.Empty;
		if (ComponentOpenData.ComponentType == ComponentType.SelectUnset)
		{
			text = "Customize/Product/Portrait/Customize_Empty_Slot";
		}
		else if (ComponentOpenData.ComponentType == ComponentType.SelectCostume && ComponentOpenData.AccessoryCostumeData != null)
		{
			text = ComponentOpenData.AccessoryCostumeData.ProductImagePath;
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.SetBundleTexture(CostumeBundleTex, text);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_listener = null;
	}

	public void OnClick()
	{
		if (ComponentOpenData.ComponentType != ComponentType.NoProduct && _listener != null)
		{
			_listener.OnClick(this);
		}
	}

	private static bool CanBuySubStoryCostume(CostumeData costumeData)
	{
		if (costumeData == null)
		{
			return false;
		}
		if (costumeData.CostumeIndex == 0)
		{
			return false;
		}
		IList<SubStoryData> allData = GameRoot.Instance.SubStoryDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			if (allData[i].UnlockCostumeIndex == costumeData.CostumeIndex && allData[i].CreatureIndex == costumeData.HeroIndex)
			{
				return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(allData[i].ChapterIndex, allData[i].DungeonIndex);
			}
		}
		return false;
	}

	public void SetSelected(bool Selected)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Selected);
	}
}
