using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class WeaponCostumeShopItemComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickBuy(WeaponCostumeShopItemComponent This);
	}

	[Serializable]
	public class RootInfo
	{
		public CostumeState[] States;

		public Transform Root;
	}

	[Serializable]
	public class SaleTagInfo
	{
		public int DiscountPercentage;

		public UISprite Sprite_Percentage;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/WeaponCostumeShopItemComponent";

	public static PrefabAuto<WeaponCostumeShopItemComponent> PrefabAuto = new PrefabAuto<WeaponCostumeShopItemComponent>("Resources/Prefab2/Component/ShopItem/WeaponCostumeShopItemComponent");

	public UIBundleLoader PortraitBundleTex;

	public List<HeroPortraitComponent.ClassTagInfo> ClassTagInfos;

	public UILabel Label_CostumeName;

	public RootInfo[] RootInfos;

	public UILabel Label_Price;

	public UIWidget WidgetNew;

	public UIWidget WidgetSale;

	public SaleTagInfo[] SaleTagInfos;

	public UILabel Label_DiscountPrice;

	private CreatureData _CreatureData;

	private Listener _listener;

	public WeaponCostumeData CostumeData { get; private set; }

	public void Open(Listener listener, WeaponCostumeData costumeData)
	{
		_listener = listener;
		CostumeData = costumeData;
		_CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(CostumeData.HeroIndex);
		Refresh();
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
			UIHelper.SetLabelText(Label_CostumeName, CostumeData.Name);
			HeroPortraitComponent.SetClassTag(_CreatureData, ClassTagInfos);
			UIHelper.EnableGameObject(WidgetNew, CostumeData.IsNew);
		}
	}

	private void SetupCostumeInfos()
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(CostumeData.HeroIndex);
		CostumeState costumeState = WeaponCostume.GetCostumeState(heroInfo, CostumeData);
		for (int i = 0; i < RootInfos.Length; i++)
		{
			UIHelper.EnableGameObject(RootInfos[i].Root, Array.FindIndex(RootInfos[i].States, (CostumeState state) => state == costumeState) != -1);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickBuy(this);
		}
	}
}
