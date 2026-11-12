using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopHeroSelectPopup : BaseWindow, HeroPortraitComponent.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopHeroSelectPopup";

	public UIScrollGrid HeroListGrid;

	private Action<int> _callback;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public static void OpenPopup(List<CreatureData> heroList, Action<int> callback)
	{
		WindowPrefab<PayShopHeroSelectPopup> windowPrefab = new WindowPrefab<PayShopHeroSelectPopup>("Resources/Prefab2/Window/PayShop/PayShopHeroSelectPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open(heroList, callback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void Open(List<CreatureData> heroList, Action<int> callback)
	{
		base.Open();
		_callback = callback;
		CreateHeroList(heroList);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
	}

	private void CreateHeroList(List<CreatureData> heroList)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (CreatureData hero in heroList)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(hero.Index);
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = new CachedHeroInfo(hero);
			}
			list.Add(cachedHeroInfo);
		}
		foreach (CachedHeroInfo item in list)
		{
			if (item != null)
			{
				HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
				heroPortraitComponent.Open(item, this, null, HeroPortraitComponent.EPortraitMode.Icon);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(item.HeroIndex) == null;
				if (flag)
				{
					heroPortraitComponent.SetNotOwnedSmall();
				}
				heroPortraitComponent.SetLevel(!flag);
				_heroPortraitList.Add(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		Vespa_System_Action_Extension_Method.fire(_callback, heroPortrait.HeroInfo.HeroIndex);
		Close();
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

	public void OnClickClose()
	{
		Close();
	}
}
