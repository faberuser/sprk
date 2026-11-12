using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurifyHeroSelect : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		void EnterPurifyRoom(CachedHeroInfo heroInfo);

		void ReOpen();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PurifyHero/PurifyHeroSelect";

	public UIToggle Toggle_Awake;

	public UIToggle Toggle_Transcend;

	public Transform ToggleGrid;

	public UILabel LabelPurifyDesc;

	public UILabel LabelPurifyHero;

	public UILabel LabelPurifyTranscendDesc;

	public UILabel LabelPurifyTranscendHero;

	public UILabel Label_NoHero;

	public UIScrollGrid HeroListGrid;

	public UIWidget WidgetNoHeroSelect;

	private List<HeroPortraitComponent> _ListPurifyHeroes = new List<HeroPortraitComponent>();

	private Listener _listener;

	private HeroAwakeType _toggle;

	public override void Reset()
	{
		_listener = null;
		FactoryBase.ReleaseAndClearList(_ListPurifyHeroes);
	}

	public void Open(Listener listener, HeroAwakeType heroAwakeType, CachedHeroInfo selectedHeroInfo)
	{
		Open(delegate
		{
			Init(listener, heroAwakeType, selectedHeroInfo);
		}, null);
	}

	public void ReOpen(Listener listener)
	{
		Open(delegate
		{
			Init(listener, _toggle, null);
		}, null);
	}

	public void Init(Listener listener, HeroAwakeType heroAwakeType, CachedHeroInfo selectedHeroInfo)
	{
		_listener = listener;
		_toggle = heroAwakeType;
		SetDefaultToggle();
		SetupPurifiableHeroesList();
		SetupSelectHeroDesc();
		UIHelper.EnableGameObject(ToggleGrid, enable: true);
		Toggle_Awake.Set(_toggle == HeroAwakeType.Awaken);
		Toggle_Transcend.Set(_toggle == HeroAwakeType.Transcend);
		EnterPurifyRoomShortCut(selectedHeroInfo);
	}

	private void SetupPurifiableHeroesList()
	{
		List<CachedHeroInfo> purifiableHeroes = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetPurifiableHeroes(_toggle == HeroAwakeType.Transcend);
		purifiableHeroes.Sort(delegate(CachedHeroInfo l, CachedHeroInfo r)
		{
			int num = l.Purified.CompareTo(r.Purified);
			if (num != 0)
			{
				return num;
			}
			num = l.Star.CompareTo(r.Star);
			return (num != 0) ? num : l.HeroIndex.CompareTo(r.HeroIndex);
		});
		SetupPurifiableHeroesList(purifiableHeroes);
	}

	public void SetupPurifiableHeroesList(IList<CachedHeroInfo> heroInfos)
	{
		FactoryBase.ReleaseAndClearList(_ListPurifyHeroes);
		UIHelper.EnableGameObject(Label_NoHero, __Extension_Method_ICollection__.COUNT(heroInfos) == 0);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
		{
			CreatePurifyHeroComponent(heroInfos[i]);
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreatePurifyHeroComponent(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
			cachedHeroInfo.HeroIndex = heroInfo.HeroIndex;
			if (_toggle == HeroAwakeType.Awaken)
			{
				cachedHeroInfo.Star = (sbyte)(heroInfo.Star + 1);
				cachedHeroInfo.Transcended = heroInfo.Transcended;
			}
			else
			{
				cachedHeroInfo.Star = heroInfo.Star;
				cachedHeroInfo.Transcended = (sbyte)(heroInfo.Transcended + 1);
			}
			cachedHeroInfo.Level = heroInfo.Level;
			cachedHeroInfo.Purified = heroInfo.Purified;
			cachedHeroInfo.CostumeIndex = heroInfo.CostumeIndex;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.PrefabAuto);
			heroPortraitComponent.Open(cachedHeroInfo, this);
			heroPortraitComponent.ShowCreatureName();
			float num = PurifyHeroRoom.GetMaxPurified(heroInfo.Star, heroInfo.Transcended);
			float purifyGauge = (float)cachedHeroInfo.Purified / num;
			heroPortraitComponent.SetPurifyGauge(purifyGauge);
			_ListPurifyHeroes.Add(heroPortraitComponent);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (_listener != null)
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(comp.HeroInfo.HeroIndex);
			_listener.EnterPurifyRoom(heroInfo);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool IsMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	private void SetupSelectHeroDesc()
	{
		UIHelper.EnableGameObject(LabelPurifyDesc, _toggle == HeroAwakeType.Awaken);
		UIHelper.EnableGameObject(LabelPurifyHero, _toggle == HeroAwakeType.Awaken);
		UIHelper.EnableGameObject(LabelPurifyTranscendDesc, _toggle == HeroAwakeType.Transcend);
		UIHelper.EnableGameObject(LabelPurifyTranscendHero, _toggle == HeroAwakeType.Transcend);
	}

	public void OnClickEnterPurifyRoom()
	{
	}

	public void OnChangeToggle(UIToggle toggle)
	{
		if (toggle.value)
		{
			if (Toggle_Transcend == toggle)
			{
				_toggle = HeroAwakeType.Transcend;
			}
			else if (Toggle_Awake == toggle)
			{
				_toggle = HeroAwakeType.Awaken;
			}
			SetupPurifiableHeroesList();
			SetupSelectHeroDesc();
		}
	}

	public override void ReOpen()
	{
		if (_listener != null)
		{
			_listener.ReOpen();
		}
	}

	private void SetDefaultToggle()
	{
		if (_toggle == HeroAwakeType.Awaken)
		{
			int num = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetPurifiableHeroes());
			int num2 = __Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetPurifiableHeroes(bTranscended: true));
			if (num == 0 && num2 > 0)
			{
				_toggle = HeroAwakeType.Transcend;
			}
		}
	}

	private void EnterPurifyRoomShortCut(CachedHeroInfo selectedHeroInfo)
	{
		if (selectedHeroInfo != null && __Extension_Method_ICollection__.COUNT(_ListPurifyHeroes) > 0)
		{
			HeroPortraitComponent heroPortraitComponent = _ListPurifyHeroes.Find((HeroPortraitComponent data) => data.HeroInfo.HeroIndex == selectedHeroInfo.HeroIndex);
			if (heroPortraitComponent != null)
			{
				heroPortraitComponent.OnClickPortrait();
				HeroListGrid.ScrollTo(heroPortraitComponent, 0f, 0f);
			}
		}
	}

	public void SetupTutorialLayout()
	{
		_toggle = HeroAwakeType.Awaken;
		SetupSelectHeroDesc();
	}
}
