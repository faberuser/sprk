using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroPortraitList : BaseWindow, HeroPortraitComponent.Listener
{
	public interface Listener
	{
		bool OnCloseRequest(HeroPortraitList caller);

		void OnClick(HeroPortraitComponent comp);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroPortraitList";

	public static PrefabAuto<HeroPortraitList> prefabAuto = new PrefabAuto<HeroPortraitList>("Resources/Prefab2/Window/Hero/HeroPortraitList");

	public UIScrollGrid HeroListGrid;

	private Listener _listener;

	private int _SelectedHeroIndex;

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	public void Open(Listener listener, int heroIndex)
	{
		SprkTicketSearch.BeforeHeroGrid(this);
		base.Open();
		_listener = listener;
		_SelectedHeroIndex = heroIndex;
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true).ForEach(delegate(CachedHeroInfo heroInfo)
		{
			CreateHeroItem(heroInfo);
		});
		UIHelper.RepositionGrid(HeroListGrid);
		SprkTicketSearch.AfterHeroGrid(this);
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
			if (heroInfo.BookmarkPriority > 0)
			{
				heroPortraitComponent.SetBookmarkedSmall(select: true);
			}
			heroPortraitComponent.SetDispatched(DispatchHelper.IsDispatchedHero(heroInfo), isSmall: true);
			_heroPortraitList.Add(heroPortraitComponent);
		}
	}

	public void MoveGridItem(int from, int to, bool bookmarked)
	{
		if (SprkTicketSearch.MoveHeroGrid(this, from, to, bookmarked) && !(HeroListGrid == null) && !(HeroListGrid.Grid == null))
		{
			int count = HeroListGrid.Grid.GetChildList().Count;
			if (count > from && count > to)
			{
				Transform child = HeroListGrid.Grid.GetChild(from);
				child.GetComponent<HeroPortraitComponent>().SetBookmarkedSmall(bookmarked);
				child.SetSiblingIndex(to);
				UIHelper.RepositionGrid(HeroListGrid);
			}
		}
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		if (_listener != null)
		{
			_listener.OnClick(comp);
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
