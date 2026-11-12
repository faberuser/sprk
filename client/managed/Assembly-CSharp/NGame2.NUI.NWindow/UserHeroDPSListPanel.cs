using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class UserHeroDPSListPanel
{
	private enum EDPSToggle
	{
		Melee,
		Range
	}

	[Serializable]
	public class PositionToggleGroup
	{
		public UIToggle Melee;

		public UIToggle Range;

		public bool IsValidToggle()
		{
			if (Melee == null || Range == null)
			{
				return false;
			}
			return true;
		}
	}

	public interface IListener
	{
		bool IsRestrictedHero(HeroInfo heroInfo);

		bool hasAcceptedHeroes();

		bool hasBannedHeroes();
	}

	public class HeroInfoKey : Tuple<long, int>
	{
		public HeroInfoKey(long AccountId, int HeroIndex)
			: base(AccountId, HeroIndex)
		{
		}
	}

	public PositionToggleGroup TogglePosition;

	public UIScrollGrid GridHeroList;

	protected Dictionary<HeroInfoKey, HeroPortraitComponent> _dicUserHeroPortarit = new Dictionary<HeroInfoKey, HeroPortraitComponent>();

	protected HashSet<string> _hashReservedHeroSelectVoice = new HashSet<string>();

	private EDPSToggle _toggleTab;

	protected HeroPortraitComponent.Listener _heroPortraitListener;

	protected IListener _listener;

	protected Action<HeroPortraitComponent> _selectionMarkAction;

	protected EnterDungeonData _dungeonData;

	public void Release()
	{
		ReleaseHeroPortraitComponent();
		_selectionMarkAction = null;
		_heroPortraitListener = null;
	}

	protected void ReleaseHeroPortraitComponent()
	{
		FactoryBase.ReleaseAndClearList(_dicUserHeroPortarit);
	}

	public void InitHeroListPanel(EnterDungeonData dungeonData, HeroPortraitComponent.Listener listener, Action<HeroPortraitComponent> selectionMarkAction)
	{
		_heroPortraitListener = listener;
		_listener = listener as IListener;
		_selectionMarkAction = selectionMarkAction;
		_dungeonData = dungeonData;
		if (IsValidTogglePosition() && !TogglePosition.Melee.value)
		{
			TogglePosition.Melee.value = true;
		}
		else
		{
			SetUserHeroList();
		}
	}

	public void SetUserHeroList()
	{
		ReleaseHeroPortraitComponent();
		int chapter = _dungeonData.Chapter;
		int linkedDungeon = _dungeonData.LinkedDungeon;
		DungeonDifficultyType difficulty = _dungeonData.Difficulty;
		bool isScenario = _dungeonData.IsScenario;
		CreatureDPSType dPSType = GetDPSType(_toggleTab);
		bool flag = Helper.CheckFixedSetHero(chapter, linkedDungeon, difficulty, isScenario);
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(flag);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
		{
			return;
		}
		if (_listener != null && (_listener.hasAcceptedHeroes() || _listener.hasBannedHeroes()))
		{
			heroInfoList.Sort(SortedHero);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfoList); i++)
		{
			CachedHeroInfo cachedHeroInfo = heroInfoList[i];
			if (cachedHeroInfo == null)
			{
				continue;
			}
			if (flag)
			{
				HeroInfo dungeonHeroSetHeroInfo = Helper.GetDungeonHeroSetHeroInfo(chapter, linkedDungeon, difficulty, cachedHeroInfo.HeroIndex, isScenario);
				if (dungeonHeroSetHeroInfo != null)
				{
					cachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
				}
			}
			bool isRestricted = _listener != null && _listener.IsRestrictedHero(cachedHeroInfo);
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(cachedHeroInfo.HeroIndex);
			if (data != null && data.GetDPSType() == dPSType)
			{
				CreateHeroPortraitComponent(cachedHeroInfo, isRestricted, flag);
			}
		}
		UIHelper.RepositionGrid(GridHeroList);
		SelectionMark();
	}

	protected void CreateHeroPortraitComponent(CachedHeroInfo heroInfo, bool isRestricted = false, bool isDungeonHeroSet = false)
	{
		if (heroInfo != null)
		{
			HeroPortraitComponent.OwnerInfo owner = new HeroPortraitComponent.OwnerInfo
			{
				AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
				Nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
				Level = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel
			};
			long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			_ = heroInfo.HeroIndex;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(GridHeroList, HeroPortraitComponent.ShortPrefabAuto);
			heroPortraitComponent.Open(heroInfo, _heroPortraitListener, owner);
			heroPortraitComponent.SetRestricted(isRestricted);
			heroPortraitComponent.EnableViewMyInfo(enable: true);
			if (isDungeonHeroSet)
			{
				heroPortraitComponent.ShowCreatureName();
			}
			_dicUserHeroPortarit.Add(new HeroInfoKey(id, heroInfo.HeroIndex), heroPortraitComponent);
			if (heroPortraitComponent.CreatureData != null)
			{
				ReservedHeroSelectVoice(heroPortraitComponent.CreatureData.CodeName);
			}
		}
	}

	private void ReservedHeroSelectVoice(string heroCode)
	{
		if (!String_Extension_Method.IsNullOrEmpty(heroCode) && !_hashReservedHeroSelectVoice.Contains(heroCode))
		{
			_hashReservedHeroSelectVoice.Add(heroCode);
			string code = DefineSound.HeroSelectSound(heroCode);
			List<IAsyncLoad<AudioClip>> list = SingletonAuto<SoundContainer>.instance.ReserveSound(code);
			SingletonAuto<AsyncSceneContainer>.instance.AddList(list);
		}
	}

	public void SelectionMark()
	{
		List<HeroPortraitComponent> list = _dicUserHeroPortarit.Values.ToList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			HeroPortraitComponent heroPortraitComponent = list[i];
			if (!(heroPortraitComponent == null))
			{
				Vespa_System_Action_Extension_Method.fire(_selectionMarkAction, heroPortraitComponent);
			}
		}
	}

	public bool IsSelectedMelee()
	{
		return TogglePosition.Melee.value;
	}

	private bool IsValidTogglePosition()
	{
		if (TogglePosition == null || !TogglePosition.IsValidToggle())
		{
			return false;
		}
		return true;
	}

	private CreatureDPSType GetDPSType(EDPSToggle tab)
	{
		return tab switch
		{
			EDPSToggle.Melee => CreatureDPSType.Melee, 
			EDPSToggle.Range => CreatureDPSType.Range, 
			_ => CreatureDPSType.Melee, 
		};
	}

	protected List<CachedHeroInfo> GetHeroInfoList(bool isDungeonHeroSet)
	{
		List<CachedHeroInfo> list = null;
		int chapter = _dungeonData.Chapter;
		int linkedDungeon = _dungeonData.LinkedDungeon;
		DungeonDifficultyType difficulty = _dungeonData.Difficulty;
		if (isDungeonHeroSet)
		{
			int heroListSetDataIndex = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(chapter, linkedDungeon, difficulty).HeroListSetDataIndex;
			if (heroListSetDataIndex != 0)
			{
				IList<DungeonHeroListSetData> datasByGroupIndex = GameRoot.Instance.DungeonHeroListSetDataContainer.GetDatasByGroupIndex(heroListSetDataIndex);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(datasByGroupIndex))
				{
					list = new List<CachedHeroInfo>();
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datasByGroupIndex); i++)
					{
						DungeonHeroListSetData dungeonHeroListSetData = datasByGroupIndex[i];
						CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
						cachedHeroInfo.HeroIndex = dungeonHeroListSetData.HeroIndex;
						cachedHeroInfo.CostumeIndex = dungeonHeroListSetData.CostumeIndex;
						list.Add(cachedHeroInfo);
					}
				}
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			list = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		}
		return list;
	}

	private int SortedHero(CachedHeroInfo l, CachedHeroInfo r)
	{
		bool flag = _listener.IsRestrictedHero(l);
		bool flag2 = _listener.IsRestrictedHero(r);
		if ((flag && flag2) || (!flag && !flag2))
		{
			int num = -l.BookmarkPriority.CompareTo(r.BookmarkPriority);
			if (num != 0)
			{
				return num;
			}
			num = -l.Transcended.CompareTo(r.Transcended);
			if (num != 0)
			{
				return num;
			}
			num = -l.Level.CompareTo(r.Level);
			if (num != 0)
			{
				return num;
			}
			num = -l.Star.CompareTo(r.Star);
			if (num != 0)
			{
				return num;
			}
			return l.HeroIndex.CompareTo(r.HeroIndex);
		}
		if (!flag)
		{
			return -1;
		}
		if (!flag2)
		{
			return 1;
		}
		return 0;
	}

	public void SetDPSToggle(bool isMelee)
	{
		TogglePosition.Melee.value = isMelee;
		TogglePosition.Range.value = !isMelee;
		OnDPSToggleChange();
	}

	public bool OnDPSToggleChange()
	{
		EDPSToggle toggleTab = _toggleTab;
		if (!IsValidTogglePosition())
		{
			return false;
		}
		if (TogglePosition.Melee.value)
		{
			_toggleTab = EDPSToggle.Melee;
		}
		else if (TogglePosition.Range.value)
		{
			_toggleTab = EDPSToggle.Range;
		}
		if (toggleTab == _toggleTab)
		{
			return false;
		}
		SetUserHeroList();
		return true;
	}

	public HeroPortraitComponent GetUserHeroPortraitComponent(long ownerAccountId, int heroIndex)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicUserHeroPortarit, new HeroInfoKey(ownerAccountId, heroIndex), null);
	}
}
