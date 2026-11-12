using System;
using System.Collections.Generic;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GlobalMatchRanking : BaseWindow, MatchRankerComponent.Listener, RecordsOfHonorShortcut.ServerTypeProvider
{
	public enum EToggle
	{
		None,
		World,
		WorldPrevSeason
	}

	private enum TierPageDirection
	{
		Prev,
		Next
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Match/GlobalMatchRanking";

	public UIToggle Toggle_WorldRanking;

	public UIToggle Toggle_WorldPrevSeasonRanking;

	public UIScrollGrid RankerGrid;

	public UILabel TierLabel;

	public UILabel EmptyLabel;

	public UILabel PageLabel;

	public UIButton PrevTierBtn;

	public UIButton NextTierBtn;

	public UIButton PrevPageBtn;

	public UIButton NextPageBtn;

	public UIButton Button_History;

	public Transform PageSelect;

	public Transform TierSelect;

	private MatchTierType _tierType;

	private MatchPvpType _pvpType;

	private int _page;

	private bool _isFirstUpdate = true;

	private int _maxPageCount = 5;

	private EToggle _toggle;

	private List<MatchRankerComponent> _componentList = new List<MatchRankerComponent>();

	public void Init(MatchPvpType pvpType, MatchTierType initialTierType, int initialPage = 0)
	{
		_maxPageCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("RankerMaxPageCount");
		_pvpType = pvpType;
		_tierType = initialTierType;
		_isFirstUpdate = true;
		string stringDirect = LocalizationManager.GetStringDirect("UI_RANKERS_EMPTY");
		UIHelper.SetLabelText(EmptyLabel, stringDirect);
		InitToggle(EToggle.World);
		if (_pvpType == MatchPvpType.BanPick)
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(_pvpType);
			if (matchSeasonInfo != null && matchSeasonInfo.MatchSeasonType == MatchSeasonType.Regular)
			{
				SceneAutoBehaviour<MatchManagement>.instance.CheckFirstViewSeasonReward_BanPick();
			}
		}
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		_isFirstUpdate = true;
		_tierType = MatchTierType.Bronze;
		InitToggle(EToggle.None);
		_toggle = EToggle.None;
		UIHelper.EnableGameObject(PrevTierBtn, enable: true);
		UIHelper.EnableGameObject(NextTierBtn, enable: true);
		UIHelper.EnableGameObject(PageSelect, enable: true);
		ClearRankerList();
	}

	private void ClearRankerList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void RefreshTierRanker(MatchTierType tierType, int page)
	{
		_tierType = tierType;
		_page = page;
		UIHelper.EnableGameObject(TierSelect, enable: true);
		UIHelper.EnableGameObject(PageSelect, enable: true);
		string text = LocalizationManager.GetString(tierType) + " " + LocalizationManager.GetString(Match.LEAGUE);
		UIHelper.SetLabelText(TierLabel, text);
		UIHelper.SetLabelText(PageLabel, page + 1 + "/" + _maxPageCount);
		string stringDirect = LocalizationManager.GetStringDirect("UI_RANKERS_EMPTY");
		UIHelper.SetLabelText(EmptyLabel, stringDirect);
		if (_toggle == EToggle.World)
		{
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestCurrentRankerList(_pvpType, tierType, _page, RefreshRankerList);
		}
		else if (_toggle == EToggle.WorldPrevSeason)
		{
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestPrevRankerList(_pvpType, tierType, _page, RefreshRankerList);
		}
	}

	private void RefreshRankerList(MatchRankerInfo[] rankerInfos)
	{
		ClearRankerList();
		if (rankerInfos == null)
		{
			UIHelper.EnableGameObject(EmptyLabel, enable: true);
			return;
		}
		UIHelper.EnableGameObject(EmptyLabel, enable: false);
		foreach (MatchRankerInfo rankerInfo in rankerInfos)
		{
			MatchRankerComponent matchRankerComponent = UIHelper.AddGridItem(RankerGrid, MatchRankerComponent.PrefabAuto);
			if (matchRankerComponent == null || !matchRankerComponent.Open(rankerInfo, _pvpType, this, isGlobal: true))
			{
				FactoryBase.ReleaseComponent(matchRankerComponent);
			}
			else
			{
				_componentList.Add(matchRankerComponent);
			}
		}
		UIHelper.RepositionGrid(RankerGrid);
		UIHelper.EnableGameObject(EmptyLabel, _componentList.Count == 0);
	}

	private void RefreshMatchRankInfo(Action<MatchTierType, int> onComplete)
	{
		if (_isFirstUpdate)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, _tierType, 0);
			_isFirstUpdate = false;
		}
		else
		{
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestMatchRank(_pvpType, onComplete);
		}
	}

	private void InitToggle(EToggle startToggle)
	{
		Toggle_WorldRanking.Set(startToggle == EToggle.World);
		Toggle_WorldPrevSeasonRanking.Set(startToggle == EToggle.WorldPrevSeason);
	}

	public void GetTierTypeAndPage(out MatchTierType tierType, out int page)
	{
		tierType = _tierType;
		page = _page;
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (Toggle_WorldRanking.value)
		{
			eToggle = EToggle.World;
		}
		else if (Toggle_WorldPrevSeasonRanking.value)
		{
			eToggle = EToggle.WorldPrevSeason;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			ClearRankerList();
			RefreshMatchRankInfo(RefreshTierRanker);
		}
	}

	private void ChangeTierType(TierPageDirection direction)
	{
		IList<MatchTierType> tierList = NGame2.NContentsDefine.MatchArena.GetTierList(_pvpType.ToMatchArenaType());
		int num = tierList.IndexOf(_tierType);
		if (num < 0)
		{
			RefreshTierRanker(MatchTierType.Bronze, 0);
			return;
		}
		MatchTierType tierType;
		if (direction == TierPageDirection.Next)
		{
			int index = num + 1;
			tierType = (Vespa_Generic_IList_Extension_Method.IsValidIndex(tierList, index) ? tierList[index] : MatchTierType.Bronze);
		}
		else
		{
			int index2 = num - 1;
			tierType = ((!Vespa_Generic_IList_Extension_Method.IsValidIndex(tierList, index2)) ? MatchTierType.Challenger : tierList[index2]);
		}
		RefreshTierRanker(tierType, 0);
	}

	public void OnClickNextTier()
	{
		ChangeTierType(TierPageDirection.Next);
	}

	public void OnClickPrevTier()
	{
		ChangeTierType(TierPageDirection.Prev);
	}

	public void OnClickNextPage()
	{
		if (_page < _maxPageCount - 1)
		{
			int page = _page + 1;
			RefreshTierRanker(_tierType, page);
		}
	}

	public void OnClickPrevPage()
	{
		if (_page > 0)
		{
			int page = _page - 1;
			RefreshTierRanker(_tierType, page);
		}
	}

	public void OnClickHistory()
	{
		if (SceneAutoBehaviour<MatchManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MatchManagement>.instance.OpenMatchHistory(_pvpType);
		}
	}

	HonoraryChamber.ServerType RecordsOfHonorShortcut.ServerTypeProvider.GetCurrentServerType()
	{
		return HonoraryChamber.ServerType.World;
	}
}
