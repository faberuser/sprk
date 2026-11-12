using System.Collections;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EclipseDeckComponent : LinkFactoryComponent, InitOnCreate, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		void OnClickStartBattle(GuildArenaDeckInfo deckInfo, CachedHeroInfo[] heroInfos);

		void OnClickDeck(EclipseDeckComponent component);

		void OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName);

		void OnClickDeckHeroPortrait(HeroPortraitComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/SoulWeapon/EclipseDeckComponent";

	public static PrefabAuto<EclipseDeckComponent> PrefabAuto = new PrefabAuto<EclipseDeckComponent>("Prefab2/Component/SoulWeapon/EclipseDeckComponent");

	public UISimpleGrid Grid_HeroIcon;

	public GameObject FocusRoot;

	private IListener _listener;

	public List<CachedHeroInfo> CachedHeroInfos = new List<CachedHeroInfo>();

	private List<HeroPortraitComponent> PortraitComponents = new List<HeroPortraitComponent>();

	public UILabel Label_TeamName;

	public GameObject FocusEffectRoot;

	public GameObject DetailViewButtonRoot;

	public GameObject DisableRoot;

	[SerializeField]
	private int _deckIndex;

	private GuildArenaDeckInfo _deckInfo;

	private bool _enableDetailView;

	[HideInInspector]
	public int DeckIndex
	{
		get
		{
			return _deckIndex;
		}
		set
		{
			_deckIndex = value;
		}
	}

	public void Init(IListener listener, int deckIndex, int slotCount, bool enableDetailView = false)
	{
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
		_listener = listener;
		_deckIndex = deckIndex;
		_enableDetailView = enableDetailView;
		SetFocus(focus: false);
		SetDisableBG(enable: false);
		CreateHeroIcons(slotCount);
		InitLabels();
		SetButtons();
	}

	private void SetButtons()
	{
		UIHelper.EnableGameObject(DetailViewButtonRoot, _enableDetailView);
	}

	private void SetHeros()
	{
		if (_deckInfo.CachedHeroInfos != null)
		{
			CachedHeroInfo[] cachedHeroInfos = _deckInfo.CachedHeroInfos;
			foreach (CachedHeroInfo heroInfo in cachedHeroInfos)
			{
				GetEmptySlot().ResetPortrait(heroInfo, null);
			}
		}
	}

	private void SetDummyHeros()
	{
		if (_deckInfo.IsNPC != 1)
		{
			return;
		}
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			portraitComponent.SetDummy(enable: true);
		}
	}

	private IEnumerator RefreshRoot()
	{
		for (int i = 0; i < 5; i++)
		{
			UIHelper.EnableGameObject(base.gameObject, enable: false);
			UIHelper.EnableGameObject(base.gameObject, enable: true);
			yield return null;
		}
	}

	public void InitLabels()
	{
		UIHelper.SetLabelText(Label_TeamName, LocalizationManager.GetStringDirect("CONTENT_ECLIPSE_HERO_PARTY_TEAM_NAME", _deckIndex));
	}

	public void CreateHeroIcons(int slotCount)
	{
		ClearHeorIcons();
		for (int i = 0; i < slotCount; i++)
		{
			if (GameRoot.Instance.CreatureDataContainer.GetData(i + 1) == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + i);
				return;
			}
			CachedHeroInfo cachedHeroInfo = null;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(Grid_HeroIcon, HeroPortraitComponent.SmallPrefabAuto);
			heroPortraitComponent.Open(cachedHeroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
			heroPortraitComponent.gameObject.SetActive(value: true);
			PortraitComponents.Add(heroPortraitComponent);
			CachedHeroInfos.Add(cachedHeroInfo);
		}
		UIHelper.RepositionGrid(Grid_HeroIcon);
	}

	private void ClearHeorIcons()
	{
		FactoryBase.ReleaseAndClearList(PortraitComponents);
		CachedHeroInfos.Clear();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
		ClearHeorIcons();
		_deckInfo = null;
	}

	public int[] GetHeroIndices()
	{
		List<int> list = new List<int>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (!(portraitComponent == null) && portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo.HeroIndex);
			}
		}
		return list.ToArray();
	}

	public void SetFocus(bool focus)
	{
		UIHelper.EnableGameObject(FocusRoot, focus);
	}

	public void SetFocusEffect(bool enable)
	{
		if (!enable || HasEmptySlot())
		{
			UIHelper.EnableGameObject(FocusEffectRoot, enable);
		}
	}

	public void SetDefeatDeck()
	{
		UIHelper.EnableCollidersInChildren(this, isEnable: false);
		SetDisableBG(enable: true);
		VespaCoroutine.Start(RefreshRoot());
	}

	public void SetDisableBG(bool enable)
	{
		UIHelper.EnableGameObject(DisableRoot, enable);
	}

	public bool IsEmptyDeck()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasEmptySlot()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo == null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasComponent(HeroPortraitComponent heroPortrait)
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent == heroPortrait)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSetHero(HeroPortraitComponent heroPortrait)
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null && heroPortrait.HeroInfo != null && portraitComponent.HeroInfo.HeroIndex == heroPortrait.HeroInfo.HeroIndex)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClickDeck()
	{
		if (_listener != null)
		{
			_listener.OnClickDeck(this);
		}
	}

	public void OnClickDeckDetailInfo()
	{
		if (_listener == null)
		{
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo);
			}
		}
		_listener.OnClickDeckDetailInfo(list.ToArray(), Label_TeamName.text);
	}

	public void OnClickStartBattle()
	{
		if (_listener == null)
		{
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo);
			}
		}
		_listener.OnClickStartBattle(_deckInfo, list.ToArray());
	}

	public HeroPortraitComponent GetEmptySlot()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo == null)
			{
				return portraitComponent;
			}
		}
		return null;
	}

	public int GetEmptySlotCount()
	{
		int num = 0;
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo == null)
			{
				num++;
			}
		}
		return num;
	}

	public List<HeroPortraitComponent> GetHeroPortraitComponents()
	{
		return PortraitComponents;
	}

	public HeroPortraitComponent GetheroPortraitComponent(int heroIndex)
	{
		HeroPortraitComponent result = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PortraitComponents); i++)
		{
			HeroPortraitComponent heroPortraitComponent = PortraitComponents[i];
			if (heroPortraitComponent != null && heroPortraitComponent.HeroInfo != null && heroPortraitComponent.HeroInfo.HeroIndex == heroIndex)
			{
				result = heroPortraitComponent;
				break;
			}
		}
		return result;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.OnClickDeckHeroPortrait(heroPortrait);
		}
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
