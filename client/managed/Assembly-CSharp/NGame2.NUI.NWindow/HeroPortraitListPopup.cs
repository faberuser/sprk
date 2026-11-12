using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class HeroPortraitListPopup : BaseWindow, HeroPortraitComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroPortraitListPopup";

	private static WindowPrefab<HeroPortraitListPopup> _windowPrefab;

	public UIScrollGrid HeroListGrid;

	private int _SelectedHeroIndex;

	private Action<int> _onSelect;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public static HeroPortraitListPopup instance
	{
		get
		{
			_windowPrefab = _windowPrefab ?? new WindowPrefab<HeroPortraitListPopup>("Resources/Prefab2/Window/Hero/HeroPortraitListPopup", GameUIRoot.ELayerType.UI);
			_windowPrefab.instance.gameObject.SetActive(value: false);
			return _windowPrefab.instance;
		}
	}

	public static void OpenMyHero(int heroIndex, Action<int> onSelect)
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		instance.OpenInternal(heroInfoList, heroIndex, onSelect);
	}

	public static void OpenAllHero(int heroIndex, Action<int> onSelect)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		List<CreatureData> openedHeroList = GameRoot.Instance.CreatureDataContainer.GetOpenedHeroList();
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (!openedHeroList.Any((CreatureData e) => e.Index == heroInfo.HeroIndex))
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
				if (data != null)
				{
					openedHeroList.Add(data);
				}
			}
		}
		openedHeroList.Sort((CreatureData l, CreatureData r) => l.Index.CompareTo(r.Index));
		foreach (CreatureData item in openedHeroList)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.Index);
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = new CachedHeroInfo(item);
			}
			list.Add(cachedHeroInfo);
		}
		instance.OpenInternal(list, heroIndex, onSelect);
	}

	protected void OpenInternal(IList<CachedHeroInfo> heroInfos, int heroIndex, Action<int> onSelect)
	{
		base.Open();
		_SelectedHeroIndex = heroIndex;
		_onSelect = onSelect;
		InitPotraitList(heroInfos);
	}

	public override void Reset()
	{
		base.Reset();
		_SelectedHeroIndex = 0;
		_onSelect = null;
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void InitPotraitList(IList<CachedHeroInfo> heroInfos)
	{
		foreach (CachedHeroInfo heroInfo in heroInfos)
		{
			CreateHeroItem(heroInfo);
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreateHeroItem(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
			heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
			if (_SelectedHeroIndex == heroInfo.HeroIndex)
			{
				heroPortraitComponent.SetSelectSmall(select: true);
			}
			_heroPortraitList.Add(heroPortraitComponent);
		}
	}

	public void OnConfirm()
	{
		Vespa_System_Action_Extension_Method.fire(_onSelect, _SelectedHeroIndex);
		Close();
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		int selectedHeroIndex = ((comp != null && comp.HeroInfo != null) ? comp.HeroInfo.HeroIndex : 0);
		if (selectedHeroIndex == _SelectedHeroIndex)
		{
			return;
		}
		HeroPortraitComponent heroPortraitComponent = _heroPortraitList.FirstOrDefault((HeroPortraitComponent e) => e.HeroInfo != null && e.HeroInfo.HeroIndex == selectedHeroIndex);
		if (heroPortraitComponent != null)
		{
			HeroPortraitComponent heroPortraitComponent2 = _heroPortraitList.FirstOrDefault((HeroPortraitComponent e) => e.HeroInfo != null && e.HeroInfo.HeroIndex == _SelectedHeroIndex);
			if ((bool)heroPortraitComponent2)
			{
				heroPortraitComponent2.SetSelectSmall(select: false);
			}
			heroPortraitComponent.SetSelectSmall(select: true);
			_SelectedHeroIndex = selectedHeroIndex;
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}
}
