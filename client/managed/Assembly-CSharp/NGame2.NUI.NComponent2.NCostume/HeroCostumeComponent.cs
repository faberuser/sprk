using System;
using NGame2.NUI.NManager;
using NGame2.NUtil.NCostume;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NCostume;

public class HeroCostumeComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(HeroCostumeComponent component);

		void OnClickChangeCos(int costumeIndex);
	}

	[Serializable]
	public class RootInfo
	{
		public CostumeState State;

		public Transform Root;
	}

	public const string PrefabPath = "Prefab2/Component/Costume/HeroCostumeComponent";

	public static PrefabAuto<HeroCostumeComponent> PrefabAuto = new PrefabAuto<HeroCostumeComponent>("Prefab2/Component/Costume/HeroCostumeComponent");

	public Transform ComingSoon;

	public UIWidget Widget_Root;

	public RootInfo[] RootInfos;

	public UILabel NameLabel;

	public UILabel ReqGemLabel;

	public UILabel DiscountReqGemLabel;

	public UIWidget WidgetSaleTag;

	public CostumeSaleTagInfo[] SaleTagInfos;

	public UILabel Label_Requirement;

	public UILabel Label_RequireSubStory;

	public UIBundleLoader CostumeBundleTex;

	public UITexture CostumeTexture;

	public UISprite Sprite_Selected;

	public UISprite Sprite_New;

	public UIButton Button_ChangeCos;

	private Listener _listener;

	private bool _showComingSoonSprite;

	private bool _notLegendBuyable;

	public CostumeData CostumeData { get; private set; }

	public bool Open(CostumeData costumeData, Listener listener, bool existProduct = true)
	{
		CostumeData = costumeData;
		_listener = listener;
		InitCostumeInfo();
		InitCostumeImage();
		SetCostumePrice();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			UIHelper.EnableGameObject(RootInfos[i].Root, enable: false);
		}
		if (costumeData != null)
		{
			UIHelper.EnableGameObject(Sprite_New, costumeData.New != 0);
			UIHelper.EnableGameObject(Button_ChangeCos, CostumeData.IsCoupleCostume());
		}
		SetSelected(Selected: false);
		_showComingSoonSprite = costumeData == null && !existProduct;
		UIHelper.EnableGameObject(ComingSoon, _showComingSoonSprite);
		if (_showComingSoonSprite)
		{
			UIHelper.EnableGameObject(Sprite_New, enable: false);
		}
		return true;
	}

	public void InitCostumeInfo()
	{
		if (CostumeData != null)
		{
			UIHelper.SetLabelText(NameLabel, CostumeData.CostumeName);
			if (ClientValueManager.IsShowCostumeIndex)
			{
				UIHelper.SetLabelText(NameLabel, $"({CostumeData.CostumeIndex}){NameLabel.text}");
			}
		}
	}

	public void SetState(CostumeState state)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RootInfos); i++)
		{
			UIHelper.EnableGameObject(RootInfos[i].Root, RootInfos[i].State == state);
		}
		if (state == CostumeState.NeedAwaken)
		{
			UIHelper.SetLabelText(Label_Requirement, CostumeData.ObtainDescription);
		}
		_notLegendBuyable = state == CostumeState.LegendNotBuyable;
		UIHelper.SetWidgetGrayScale(Widget_Root, _notLegendBuyable);
		UIHelper.EnableButton(Button_ChangeCos, !_notLegendBuyable);
		if (_notLegendBuyable)
		{
			UIHelper.EnableGameObject(Sprite_New, enable: false);
		}
		bool enable = (state == CostumeState.NeedAwaken && CostumeData != null && !CostumeData.IsFreeCostume()) || state == CostumeState.Buyable;
		UIHelper.EnableGameObject(WidgetSaleTag, enable);
	}

	public void SetCostumePrice()
	{
		int priceGem = HeroCostume.GetPriceGem(CostumeData);
		long i_value = priceGem;
		int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CostumeData);
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

	private void InitCostumeImage()
	{
		if (CostumeData != null && GameRoot.Instance.CreatureDataContainer.GetData(CostumeData.HeroIndex) != null)
		{
			string battlePortraitPath = CostumeData.BattlePortraitPath;
			UIHelper.SetBundleTexture(CostumeBundleTex, battlePortraitPath);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_listener = null;
	}

	public void OnClick()
	{
		if (_listener != null && !_showComingSoonSprite && !_notLegendBuyable)
		{
			_listener.OnClick(this);
		}
	}

	public void OnClickChangeCos()
	{
		if (_listener != null && CostumeData.BonusCostumeIndices != null && CostumeData.BonusCostumeIndices.Length != 0 && UIHelper.IsEnabledGameObjet(Sprite_Selected))
		{
			int costumeIndex = CostumeData.BonusCostumeIndices[0];
			_listener.OnClickChangeCos(costumeIndex);
		}
	}

	public void SetSelected(bool Selected)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Selected);
	}

	public void ChangeCostumeData(CostumeData costumeData)
	{
		CostumeData = costumeData;
		InitCostumeInfo();
		InitCostumeImage();
	}
}
