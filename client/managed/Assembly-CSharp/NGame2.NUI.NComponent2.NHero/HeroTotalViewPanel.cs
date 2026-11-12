using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NHero;

public class HeroTotalViewPanel : MonoBehaviour, IHeroPanel, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		void OnClick(HeroPortraitComponent comp);
	}

	public const string HERO_SELECT_POPUP_TITLE_DEFAULT = "COMMON_HERO_SELECT";

	public const string HERO_SELECT_POPUP_TITLE_DEALER = "HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_TITLE_DEALER";

	public const string HERO_SELECT_POPUP_DESC_DEALER = "HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_DESC_DEALER";

	public const string HERO_SELECT_POPUP_TITLE_SUPPORTER = "HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_TITLE_SUPPORTER";

	public const string HERO_SELECT_POPUP_DESC_SUPPORTER = "HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_DESC_SUPPORTER";

	public UIScrollGrid HeroListGrid;

	public UIButton TotalViewButton;

	public HeroPortraitComponent CurrentSelectedHero;

	public UILabel Label_Title;

	public UILabel Label_Description;

	public GameObject PhysicalType_Root;

	public GameObject MagicalType_Root;

	private List<HeroPortraitComponent> _heroCompList = new List<HeroPortraitComponent>();

	private IDataProvider _dataProvider;

	private HeroPortraitComponent.Listener _listener;

	private HeroPortraitComponent selectedHeroComp;

	public int VisualStar { get; private set; }

	public int VisualTrans { get; private set; }

	public int VisualLevel { get; private set; }

	public void Open(IDataProvider dataProvider, HeroPortraitComponent.Listener listener, HeroUsageType heroUsageType, int heroStar, int heroLevel, int heroTrans)
	{
		Reset();
		base.gameObject.SetActive(value: true);
		_dataProvider = dataProvider;
		_listener = listener;
		VisualStar = heroStar;
		VisualLevel = heroLevel;
		VisualTrans = heroTrans;
		string stringDirect;
		string text;
		switch (heroUsageType)
		{
		case HeroUsageType.Dealer:
			stringDirect = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_TITLE_DEALER");
			text = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_DESC_DEALER");
			break;
		case HeroUsageType.Supporter:
			stringDirect = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_TITLE_SUPPORTER");
			text = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROPOPUP_DESC_SUPPORTER");
			break;
		default:
			stringDirect = LocalizationManager.GetStringDirect("COMMON_HERO_SELECT");
			text = string.Empty;
			break;
		}
		UIHelper.SetLabelText(Label_Title, stringDirect);
		UIHelper.SetLabelText(Label_Description, text);
		if (CurrentSelectedHero != null)
		{
			CurrentSelectedHero.Open(dataProvider.GetSelectedHeroData(), null, HeroPortraitComponent.EPortraitMode.Basic, VisualStar, VisualTrans, 0, VisualLevel);
		}
		Init();
	}

	public void Close()
	{
		Reset();
	}

	private void Init()
	{
		SetHeroList();
	}

	public void Reset()
	{
		_dataProvider = null;
		_listener = null;
		CurrentSelectedHero.Close();
		ClearHeroList();
		base.gameObject.SetActive(value: false);
	}

	private void SetHeroList()
	{
		ClearHeroList();
		List<CreatureData> heroDataList = _dataProvider.GetHeroDataList();
		SkillAttrType recommandedType = _dataProvider.GetRecommandedType();
		bool level = VisualLevel > 1;
		foreach (CreatureData item in heroDataList)
		{
			if (item != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
				bool isRecommanded = recommandedType == item.HeroAttrType;
				heroPortraitComponent.Open(item, this, HeroPortraitComponent.EPortraitMode.Icon, VisualStar, 0, 0, VisualLevel, isRecommanded);
				if (_dataProvider.GetSelectedHeroData() == item)
				{
					selectedHeroComp = heroPortraitComponent;
					heroPortraitComponent.SetSelectSmall(select: true);
					SetCurrentSelectedPortrait(item);
				}
				heroPortraitComponent.SetLevel(level);
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.Index) != null)
				{
					heroPortraitComponent.SetOwned(owned: true);
				}
				_heroCompList.Add(heroPortraitComponent);
			}
		}
		UIHelper.ResetGridPosition(HeroListGrid);
	}

	private void SetCurrentSelectedPortrait(CreatureData heroData)
	{
		bool level = VisualLevel > 1;
		bool show = heroData.HeroAttrType == _dataProvider.GetRecommandedType();
		CurrentSelectedHero.ResetPortrait(null, heroData);
		CurrentSelectedHero.ShowName(heroData.Name);
		CurrentSelectedHero.SetLevel(level);
		CurrentSelectedHero.ShowRecommandIcon(show);
		switch (heroData.HeroAttrType)
		{
		case SkillAttrType.Physical:
			UIHelper.EnableGameObject(PhysicalType_Root, enable: true);
			UIHelper.EnableGameObject(MagicalType_Root, enable: false);
			break;
		case SkillAttrType.Magical:
			UIHelper.EnableGameObject(MagicalType_Root, enable: true);
			UIHelper.EnableGameObject(PhysicalType_Root, enable: false);
			break;
		default:
			UIHelper.EnableGameObject(PhysicalType_Root, enable: false);
			UIHelper.EnableGameObject(MagicalType_Root, enable: false);
			break;
		}
	}

	public void OnClickClose()
	{
		base.gameObject.SetActive(value: false);
	}

	private void ClearHeroList()
	{
		FactoryBase.ReleaseAndClearList(_heroCompList);
	}

	public void OnClickSelectHero()
	{
		if (_listener != null)
		{
			_listener.OnClick(selectedHeroComp);
		}
		base.gameObject.SetActive(value: false);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent clickedPortrait)
	{
		selectedHeroComp.SetSelectSmall(select: false);
		selectedHeroComp = clickedPortrait;
		selectedHeroComp.SetSelectSmall(select: true);
		SetCurrentSelectedPortrait(clickedPortrait.CreatureData);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}
}
