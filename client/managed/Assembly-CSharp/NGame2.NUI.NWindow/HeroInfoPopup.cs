using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NHero;
using NGame2.NUI.NManager.NLobby;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroInfoPopup : BaseWindow, IDataProvider, HeroTotalViewPanel.IListener, HeroPortraitComponent.Listener, ArchiveHeroProfilePanel.Listener
{
	public interface IListener
	{
		void OnClickBuy(CreatureData creatureData);

		void OnClickViewHeroDetail(CachedHeroInfo heroInfo);

		void OnTryBattle(HeroInfo heroInfo);
	}

	public const int HeroVisualStar = 5;

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/MultipleHeroSelect/HeroInfoPopup";

	public const string HERO_SELECT_POPUP_TITLE_DEFAULT = "COMMON_HERO_SELECT";

	public const string HERO_SELECT_POPUP_TITLE_DEALER = "HERO_SELECT_ITEM_BEGINNER_HEROSELECTION_DESC_DEALER";

	public const string HERO_SELECT_POPUP_TITLE_SUPPORTER = "HERO_SELECT_ITEM_BEGINNER_HEROSELECTION_DESC_SUPPORTER";

	private IListener _listener;

	[Header("UI Component")]
	public UILabel TitleLabel;

	public UIBundleLoader BackgroundLoader;

	[Header("Hero Bounus")]
	public Transform PricePosition;

	public UILabel ObtainLabel;

	public Transform BonusStaminaRoot;

	public UILabel BonusStaminaLabel;

	public Transform BonusMileageRoot;

	public UILabel BonusMileageLabel;

	[Header("Button")]
	public GameObject ButtonProfile;

	public GameObject ButtonHeroView;

	public GameObject ButtonHeroBattle;

	public GameObject ButtonYes;

	[Header("UI Panel Component")]
	public HeroPortraitListPanel HeroPortraitListPanel;

	public HeroInfoPanel HeroInfoPanel;

	public HeroTotalViewPanel HeroTotalViewPanel;

	public ArchiveHeroProfilePanel HeroProfilePanel;

	public HeroModelPanel HeroModelPanel;

	public float NPCActionDelaySecond = 5f;

	public float NPCChangeDelaySecond = 0.5f;

	private CachedHeroInfo _heroInfo;

	private List<CreatureData> _heroList;

	private string titleLabelText;

	private int _heroChangedCount;

	public int RefreshGCCallCount = 5;

	public CreatureData HeroData { get; private set; }

	public int HeroStar { get; private set; }

	public int HeroTranscended { get; private set; }

	public int HeroLevel { get; private set; }

	public int CurrentIndex { get; private set; }

	public HeroUsageType HeroUsageType { get; private set; }

	public SkillAttrType RecommandedType { get; private set; }

	public void Open(CreatureData selectedHero, List<CreatureData> heroList, IListener listener, int heroLevel, int heroStar, int heroTrans, SkillAttrType recommanedType = SkillAttrType.None, HeroUsageType heroUsageType = HeroUsageType.None, bool showTotalListPanel = false)
	{
		base.Open(delegate
		{
			HeroData = selectedHero;
			_listener = listener;
			_heroList = heroList;
			RecommandedType = recommanedType;
			HeroLevel = heroLevel;
			HeroStar = heroStar;
			HeroTranscended = heroTrans;
			HeroUsageType = heroUsageType;
			Init(selectedHero);
			if (showTotalListPanel)
			{
				OnClickHeroTotalView();
			}
		});
	}

	private void Init(CreatureData heroData)
	{
		HeroData = heroData;
		if (HeroData == null)
		{
			HeroData = _heroList.FirstOrDefault();
		}
		SetCachedHeroInfo(heroData);
		InitSelectHero();
		switch (HeroUsageType)
		{
		case HeroUsageType.Dealer:
			titleLabelText = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROSELECTION_DESC_DEALER");
			break;
		case HeroUsageType.Supporter:
			titleLabelText = LocalizationManager.GetStringDirect("HERO_SELECT_ITEM_BEGINNER_HEROSELECTION_DESC_SUPPORTER");
			break;
		default:
			titleLabelText = LocalizationManager.GetStringDirect("COMMON_HERO_SELECT");
			break;
		}
		UIHelper.SetLabelText(TitleLabel, titleLabelText);
	}

	private void InitSelectHero()
	{
		InitHeroPortraitPanel();
		InitHeroInfoPanel();
		SetHeroModel(HeroData);
		SetBounusResource(HeroData);
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
	}

	private void InitHeroInfoPanel()
	{
		if (!(HeroInfoPanel == null))
		{
			HeroInfoPanel.Open(_heroInfo);
		}
	}

	private void InitHeroPortraitPanel()
	{
		if (!(HeroPortraitListPanel == null))
		{
			HeroPortraitListPanel.Open(this, this, HeroStar, HeroLevel, HeroTranscended);
		}
	}

	public void SetSelectHero(CreatureData heroData)
	{
		if (heroData != null && HeroData != heroData)
		{
			PeriodicGCCollect();
			CreatureData heroData2 = HeroData;
			HeroData = heroData;
			SetCachedHeroInfo(heroData);
			SelectHeroPortrait(heroData2, heroData);
			SetHeroInfoPanel(_heroInfo);
			SetBounusResource(heroData);
			SetHeroModel(heroData);
			HeroProfilePanel.Change(_heroInfo);
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
	}

	private void SetCachedHeroInfo(CreatureData heroData)
	{
		_heroInfo = new CachedHeroInfo(heroData);
		_heroInfo.Star = (sbyte)HeroStar;
		_heroInfo.Transcended = (sbyte)HeroTranscended;
		_heroInfo.Level = (sbyte)HeroLevel;
	}

	public void SetBounusResource(CreatureData heroData)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroData.Index) != null)
		{
			UIHelper.EnableGameObject(ButtonYes, enable: false);
			UIHelper.EnableGameObject(BonusMileageRoot, enable: false);
			UIHelper.EnableGameObject(BonusStaminaRoot, enable: false);
		}
		else
		{
			SetGetHeroButton(heroData);
			SetGetHeroStaminaBounus(heroData);
		}
	}

	private void SetGetHeroButton(CreatureData creatureData)
	{
		bool enable = creatureData.OpenType == CreatureOpenType.Opened;
		UIHelper.EnableGameObject(ButtonYes, enable);
	}

	private void SetGetHeroStaminaBounus(CreatureData creatureData)
	{
		int addStamina = creatureData.AddStamina;
		string stringDirect = LocalizationManager.GetStringDirect("NEW_PAYSHOP_STAMINA_INCREASE", addStamina);
		UIHelper.EnableGameObject(BonusStaminaRoot, enable: true);
		UIHelper.SetLabelText(BonusStaminaLabel, stringDirect);
	}

	private void SetHeroModel(CreatureData heroData)
	{
		if (heroData != null && !(HeroModelPanel == null))
		{
			CloseHeroModel();
			CreatureInUIVisualData visualData = new CreatureInUIVisualData(new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, "Victory"), CreatureStock.ELoadLevel.IncludeFacial);
			HeroModelPanel.Open(heroData, 5, 0, visualData);
			SetSpecialMotion();
		}
	}

	private void SelectHeroPortrait(CreatureData oldHero, CreatureData heroData)
	{
		if (HeroPortraitListPanel != null)
		{
			HeroPortraitListPanel.SetSelectionComp(oldHero, heroData);
		}
	}

	private void SetHeroInfoPanel(HeroInfo heroInfo)
	{
		if (HeroInfoPanel != null)
		{
			HeroInfoPanel.SetHero(heroInfo);
		}
	}

	private void PeriodicGCCollect()
	{
		_heroChangedCount++;
		if (_heroChangedCount >= RefreshGCCallCount)
		{
			GCCollect();
		}
	}

	private void GCCollect()
	{
		_heroChangedCount = 0;
		SingletonUpdater<GCManager>.instance.Collect(GCManager.ECollectType.Lite);
	}

	public void EnableCreatureWidget(bool enabled)
	{
		if (!(HeroModelPanel == null) && !(HeroModelPanel.CreatureWidget == null))
		{
			UIHelper.EnableGameObject(HeroModelPanel.CreatureWidget, enabled);
		}
	}

	private void SetSpecialMotion()
	{
		if (HeroModelPanel != null)
		{
			HeroModelPanel.SetSpecialMotion();
		}
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		HeroData = null;
		if (_heroList != null)
		{
			_heroList = null;
		}
		CloseHeroModel();
		CloseAllPanel();
	}

	public override void Reset()
	{
		base.Reset();
	}

	private void CloseHeroModel()
	{
		if (HeroModelPanel != null)
		{
			HeroModelPanel.Clear();
		}
	}

	public void OnClickHeroTotalView()
	{
		if (HeroTotalViewPanel != null)
		{
			HeroTotalViewPanel.Open(this, this, HeroUsageType, HeroStar, HeroLevel, HeroTranscended);
		}
	}

	public void OnClickHero()
	{
		if (HeroModelPanel != null)
		{
			HeroModelPanel.SetSpecialMotion();
		}
	}

	public void OnClickYes()
	{
		if (_listener == null || HeroData == null)
		{
			Close();
			return;
		}
		_listener.OnClickBuy(HeroData);
		Close();
	}

	public void OnClickDetailView()
	{
		_listener.OnClickViewHeroDetail(_heroInfo);
	}

	public void OnClickTryBattle()
	{
		_listener.OnTryBattle(_heroInfo);
	}

	public void OnClickShowProfile()
	{
		if (!UIHelper.IsEnabledGameObjet(HeroProfilePanel))
		{
			HeroProfilePanel.Open(_heroInfo, this);
		}
		else
		{
			HeroProfilePanel.Close();
		}
	}

	public IEnumerable<IHeroPanel> GetHeroPanels()
	{
		yield return HeroPortraitListPanel;
		yield return HeroProfilePanel;
		yield return HeroTotalViewPanel;
		yield return HeroInfoPanel;
	}

	public void CloseAllPanel()
	{
		foreach (IHeroPanel heroPanel in GetHeroPanels())
		{
			heroPanel?.Close();
		}
	}

	void HeroTotalViewPanel.IListener.OnClick(HeroPortraitComponent comp)
	{
		SetSelectHero(comp.CreatureData);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		SetSelectHero(heroPortrait.CreatureData);
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

	CachedHeroInfo IDataProvider.GetSelectedHeroInfo()
	{
		return _heroInfo;
	}

	CreatureData IDataProvider.GetSelectedHeroData()
	{
		return HeroData;
	}

	List<CreatureData> IDataProvider.GetHeroDataList()
	{
		return _heroList;
	}

	SkillAttrType IDataProvider.GetRecommandedType()
	{
		return RecommandedType;
	}

	void ArchiveHeroProfilePanel.Listener.OpenMonthlyHeroWindow(int creatureIndex)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenMonthlyHeroWindow(creatureIndex);
	}
}
