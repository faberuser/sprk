using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class MiniPortraitSquardPanel
{
	public GameObject Root;

	public UIToggle ToggleMain;

	public UIToggle ToggleSub;

	public GameObject MiniPortraitMainRoot;

	public GameObject MiniPortraitSubRoot;

	public UISimpleGrid MiniPortraitGrid;

	public PartySlotComponent MiniPortraitSlotTemplate;

	private List<HeroPortraitComponent> _listMiniHeroPortraitComponent = new List<HeroPortraitComponent>();

	private List<PartySlotComponent> _listMiniPortraitEmptySlotComponent = new List<PartySlotComponent>();

	public void CloseRoot()
	{
		UIHelper.EnableGameObject(Root, enable: false);
	}

	public void Release()
	{
		FactoryBase.ReleaseAndClearList(_listMiniHeroPortraitComponent);
		if (__Extension_Method_ICollection__.COUNT(_listMiniPortraitEmptySlotComponent) > 0)
		{
			_listMiniPortraitEmptySlotComponent.ForEach(delegate(PartySlotComponent e)
			{
				VespaObject.DestroyGo(e);
			});
			_listMiniPortraitEmptySlotComponent.Clear();
		}
	}

	public void SetOnOffRoot(bool isOnMainSquard)
	{
		UIHelper.EnableGameObject(MiniPortraitMainRoot, isOnMainSquard);
		UIHelper.EnableGameObject(MiniPortraitSubRoot, !isOnMainSquard);
	}

	public void SetToggleValue(bool isOnMainSquard)
	{
		if (!(ToggleMain == null) && !(ToggleSub == null))
		{
			if (isOnMainSquard)
			{
				ToggleMain.value = isOnMainSquard;
			}
			else
			{
				ToggleSub.value = !isOnMainSquard;
			}
		}
	}

	public bool GetMainToggleValue()
	{
		if (ToggleMain == null)
		{
			return false;
		}
		return ToggleMain.value;
	}

	public bool GetSubToggleValue()
	{
		if (ToggleSub == null)
		{
			return false;
		}
		return ToggleSub.value;
	}

	public void SetMiniPortrait(SquardPanel squard, Squard.IDataProvider iDataProvider)
	{
		if (!(MiniPortraitGrid == null) && !(MiniPortraitSlotTemplate == null))
		{
			UIHelper.EnableGameObject(Root, enable: true);
			Release();
			List<int> heroIndexList = null;
			int slotCount = 0;
			if (squard != null)
			{
				heroIndexList = squard.GetTotalSquardHeroIndexList();
				slotCount = squard.GetSquardSlotCount();
			}
			AddSubSlotHeroList(heroIndexList, iDataProvider);
			AddSubSlotEmpty(heroIndexList, slotCount);
			UIHelper.RepositionGrid(MiniPortraitGrid);
		}
	}

	private void AddSubSlotHeroList(List<int> heroIndexList, Squard.IDataProvider iDataProvider)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList) || iDataProvider == null || iDataProvider.EnterDungeonData == null)
		{
			return;
		}
		EnterDungeonData enterDungeonData = iDataProvider.EnterDungeonData;
		CachedHeroInfo cachedHeroInfo = null;
		HeroPortraitComponent heroPortraitComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndexList); i++)
		{
			int num = heroIndexList[i];
			cachedHeroInfo = Helper.GetDungeonHeroSetHeroInfo(enterDungeonData.Chapter, enterDungeonData.LinkedDungeon, enterDungeonData.Difficulty, num, enterDungeonData.IsScenario) as CachedHeroInfo;
			if (cachedHeroInfo == null)
			{
				cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(num);
				if (cachedHeroInfo == null)
				{
					continue;
				}
			}
			heroPortraitComponent = MiniPortraitGrid.AddGridItem(HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Open(cachedHeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
				_listMiniHeroPortraitComponent.Add(heroPortraitComponent);
			}
		}
	}

	private void AddSubSlotEmpty(List<int> heroIndexList, int slotCount)
	{
		if (__Extension_Method_ICollection__.COUNT(heroIndexList) >= slotCount)
		{
			return;
		}
		int num = slotCount - ((!__Extension_Method_ICollection__.IsNullOrEmpty(heroIndexList)) ? __Extension_Method_ICollection__.COUNT(heroIndexList) : 0);
		for (int i = 0; i < num; i++)
		{
			PartySlotComponent partySlotComponent = UIHelper.AddEmptySlotTemplate(MiniPortraitGrid.Grid, MiniPortraitSlotTemplate);
			if (!(partySlotComponent == null))
			{
				partySlotComponent.transform.SetAsFirstSibling();
				_listMiniPortraitEmptySlotComponent.Add(partySlotComponent);
			}
		}
	}

	public HeroPortraitComponent GetMiniPortrait(int heroIndex)
	{
		return _listMiniHeroPortraitComponent.Find(delegate(HeroPortraitComponent comp)
		{
			if (comp.HeroInfo == null)
			{
				return false;
			}
			return (comp.HeroInfo.HeroIndex == heroIndex) ? true : false;
		});
	}
}
