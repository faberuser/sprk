using System;
using System.Collections.Generic;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserHeroManager : NShared.UserHeroManager
{
	public enum HeroChangeType
	{
		None,
		AddedHero,
		ClearedAddHero,
		Purified,
		Star,
		Transcended,
		SkillExtended,
		SkillLevelup,
		HeroExpup,
		HeroLevelup,
		FlaskItemIndex,
		FlaskExp,
		TranscendSkillSelected,
		ResetAllTranscendSkill,
		AddTranscendSkillPage,
		Costume,
		HairCostume,
		WeaponCostume,
		AccessoryCostume,
		HideUniqueWeapon,
		EnterTrialDungeon,
		OpenedEquipSlot,
		ExtendTranscendPoint,
		AddedHeros,
		GrowthStar,
		GrowthTranscended,
		AlreadyGrowth,
		RunePage,
		HeroLimitBreakLevel,
		HeroLimitBreakExp
	}

	public class ChangeHeroInfo
	{
		public HeroChangeType ChangeType;

		public int HeroIndex;

		public int PartIndex;

		public int Transcended;

		public int SkillSlotIndex;

		public byte HideUniqueWeapon;

		public int AddValue;

		public int NewValue;

		public bool ReserveWindow;

		public Action OnCloseAction;
	}

	private readonly UserObserverManager<ChangeHeroInfo> _observerManager = new UserObserverManager<ChangeHeroInfo>();

	private HashSet<int> _AddedHeroIndices = new HashSet<int>();

	public IUserObserverManager ObserverManager => _observerManager;

	public void ApplyHeroCloseness(HeroInfo info)
	{
		if (info != null)
		{
			CachedHeroInfo cachedHeroInfo = Get(info.HeroIndex);
			if (cachedHeroInfo != null)
			{
				cachedHeroInfo.ClosenessPoint = info.ClosenessPoint;
				cachedHeroInfo.LastConversationTime = info.LastConversationTime;
				cachedHeroInfo.LastGiftTime = info.LastGiftTime;
				cachedHeroInfo.LastGreetingTime = info.LastGreetingTime;
				cachedHeroInfo.LastRouletteTime = info.LastRouletteTime;
			}
		}
	}

	public void ApplyHeroRunePage(HeroInfo info)
	{
		if (info != null)
		{
			CachedHeroInfo cachedHeroInfo = Get(info.HeroIndex);
			if (cachedHeroInfo != null)
			{
				cachedHeroInfo.ApplyRunePage = info.ApplyRunePage;
				_observerManager.AnnounceToObservers(new ChangeHeroInfo
				{
					ChangeType = HeroChangeType.RunePage,
					HeroIndex = cachedHeroInfo.HeroIndex
				});
			}
		}
	}

	public void ApplyHeroExp(HeroExpResultInfo[] infos)
	{
		if (infos != null)
		{
			foreach (HeroExpResultInfo info in infos)
			{
				ApplyHeroExp(info);
			}
		}
	}

	public void ApplyHeroExp(HeroExpResultInfo info)
	{
		if (info != null)
		{
			CachedHeroInfo cachedHeroInfo = Get(info.HeroIndex);
			if (cachedHeroInfo != null)
			{
				cachedHeroInfo.Exp = info.NewValue;
				cachedHeroInfo.Level = info.NewLevel;
			}
			if (info.OldLevel != info.NewLevel)
			{
				_observerManager.AnnounceToObservers(new ChangeHeroInfo
				{
					ChangeType = HeroChangeType.HeroLevelup,
					HeroIndex = info.HeroIndex,
					NewValue = info.NewLevel
				});
			}
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HeroExpup,
				HeroIndex = info.HeroIndex,
				AddValue = info.AddValue,
				NewValue = info.NewValue
			});
		}
	}

	public void ApplyHeroLevel(int heroIndex, int level)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && cachedHeroInfo.Level != level)
		{
			cachedHeroInfo.Level = (short)level;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HeroLevelup,
				HeroIndex = heroIndex,
				NewValue = level
			});
		}
	}

	public void ApplyHeroLimitBreakLevel(int heroIndex, sbyte level)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && cachedHeroInfo.LimitBreakLevel != level)
		{
			cachedHeroInfo.LimitBreakLevel = level;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HeroLimitBreakLevel,
				HeroIndex = heroIndex,
				NewValue = level
			});
		}
	}

	public void ApplyHeroLimitBreakExp(int heroIndex, long exp)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && cachedHeroInfo.LimitBreakExp != exp)
		{
			cachedHeroInfo.LimitBreakExp = exp;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HeroLimitBreakExp,
				HeroIndex = heroIndex,
				NewValue = (int)exp
			});
		}
	}

	public void Apply(FlaskEndCampaignResultInfo[] infos)
	{
		if (infos != null)
		{
			foreach (FlaskEndCampaignResultInfo info in infos)
			{
				Apply(info);
			}
		}
	}

	public void Apply(FlaskEndCampaignResultInfo info)
	{
		if (info != null)
		{
			Apply(info.Info);
		}
	}

	public void Apply(FlaskResultInfo[] infos)
	{
		if (infos != null)
		{
			foreach (FlaskResultInfo info in infos)
			{
				Apply(info);
			}
		}
	}

	public void Apply(FlaskResultInfo info)
	{
		if (info == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = Get(info.HeroIndex);
		if (cachedHeroInfo != null)
		{
			if (cachedHeroInfo.FlaskItemIndex != info.ItemIndex)
			{
				cachedHeroInfo.FlaskItemIndex = info.ItemIndex;
				_observerManager.AnnounceToObservers(new ChangeHeroInfo
				{
					ChangeType = HeroChangeType.FlaskItemIndex,
					HeroIndex = info.HeroIndex,
					NewValue = info.ItemIndex
				});
			}
			if (cachedHeroInfo.FlaskExp != info.NewExp)
			{
				cachedHeroInfo.FlaskExp = info.NewExp;
				_observerManager.AnnounceToObservers(new ChangeHeroInfo
				{
					ChangeType = HeroChangeType.FlaskExp,
					HeroIndex = info.HeroIndex,
					NewValue = info.NewExp
				});
			}
		}
	}

	public void Apply(HeroRunePageInfo info, bool isNotify = true, bool isRefresh = false)
	{
		if (info == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = Get(info.HeroIndex);
		if (cachedHeroInfo == null)
		{
			return;
		}
		PunishmentRuneOptionInfo optionInfo = null;
		if (info.EquipItemSlotIndex > 0)
		{
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(info.EquipItemSlotIndex);
			if (equipItemInfo != null)
			{
				equipItemInfo.SetEquippedHeroIndex(info.HeroIndex);
				optionInfo = equipItemInfo.PunishmentRuneOption;
			}
		}
		cachedHeroInfo.SetRunePage(info, optionInfo);
		if (isRefresh)
		{
			cachedHeroInfo.RefreshRuneInfo();
		}
		if (isNotify)
		{
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.RunePage,
				HeroIndex = info.HeroIndex
			});
		}
	}

	public void Apply(HeroRunePageInfo[] infos, bool isRefresh = false)
	{
		if (infos == null || infos.Length == 0)
		{
			return;
		}
		Dictionary<int, List<HeroRunePageInfo>> dictionary = new Dictionary<int, List<HeroRunePageInfo>>();
		HeroRunePageInfo[] array = infos;
		foreach (HeroRunePageInfo heroRunePageInfo in array)
		{
			if (!dictionary.ContainsKey(heroRunePageInfo.HeroIndex))
			{
				dictionary.Add(heroRunePageInfo.HeroIndex, new List<HeroRunePageInfo>());
			}
			dictionary[heroRunePageInfo.HeroIndex].Add(heroRunePageInfo);
		}
		foreach (KeyValuePair<int, List<HeroRunePageInfo>> item in dictionary)
		{
			int key = item.Key;
			CachedHeroInfo cachedHeroInfo = Get(key);
			if (cachedHeroInfo != null)
			{
				array = infos;
				foreach (HeroRunePageInfo info in array)
				{
					Apply(info, isNotify: false);
				}
				if (isRefresh)
				{
					cachedHeroInfo.RefreshRuneInfo();
				}
				_observerManager.AnnounceToObservers(new ChangeHeroInfo
				{
					ChangeType = HeroChangeType.RunePage,
					HeroIndex = key
				});
			}
		}
	}

	public void AddHeroAndAnnounce(HeroInfo heroInfo, bool reserveWindow, Action onClose = null)
	{
		if (heroInfo == null)
		{
			return;
		}
		HeroChangeType changeType = GetChangeType(heroInfo);
		Add(new CachedHeroInfo(heroInfo, GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager, (int heroIndex, int accessoryIndex) => GetPositionInfo(heroIndex, accessoryIndex)));
		_AddedHeroIndices.Add(heroInfo.HeroIndex);
		if (changeType != HeroChangeType.None)
		{
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = changeType,
				AddValue = heroInfo.HeroIndex,
				NewValue = heroInfo.HeroIndex,
				ReserveWindow = reserveWindow,
				OnCloseAction = onClose
			});
			if (changeType == HeroChangeType.AddedHero)
			{
				_ = (object)null;
				_ = 19;
				_ = 1;
				_ = 0;
				_ = new object[2]
				{
					heroInfo.HeroIndex,
					Count()
				};
			}
		}
	}

	public void AddHerosAndAnnounceRecreateChangeHeroInfo(HeroInfo[] heroInfos, bool reserveWindow = true)
	{
		Dictionary<int, ChangeHeroInfo> dictionary = new Dictionary<int, ChangeHeroInfo>();
		List<ChangeHeroInfo> list = new List<ChangeHeroInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
		{
			HeroInfo heroInfo = heroInfos[i];
			HeroChangeType changeType = GetChangeType(heroInfo);
			Add(new CachedHeroInfo(heroInfo, GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager, (int heroIndex, int accessoryIndex) => GetPositionInfo(heroIndex, accessoryIndex)));
			_AddedHeroIndices.Add(heroInfo.HeroIndex);
			if (changeType != HeroChangeType.None)
			{
				if (changeType == HeroChangeType.AddedHero)
				{
					_ = (object)null;
					_ = 19;
					_ = 1;
					_ = 0;
					_ = new object[2]
					{
						heroInfo.HeroIndex,
						Count()
					};
				}
				ChangeHeroInfo changeHeroInfo = new ChangeHeroInfo
				{
					ChangeType = changeType,
					AddValue = heroInfo.HeroIndex,
					NewValue = heroInfo.HeroIndex,
					ReserveWindow = reserveWindow
				};
				if (changeType == HeroChangeType.AddedHero)
				{
					dictionary.Add(heroInfo.HeroIndex, changeHeroInfo);
				}
				else
				{
					list.Add(changeHeroInfo);
				}
			}
		}
		List<ChangeHeroInfo> announceList = RecreateAddedChangeHeroInfo(dictionary);
		AnnounceToObserver(announceList);
		AnnounceToObserver(list);
	}

	private List<ChangeHeroInfo> RecreateAddedChangeHeroInfo(Dictionary<int, ChangeHeroInfo> addedHeroChangeInfoList)
	{
		List<int> list = new List<int>(addedHeroChangeInfoList.Keys);
		list.Sort((int l, int r) => l.CompareTo(r));
		GetNewHeroSceneBundleData data = GameRoot.Instance.GetNewHeroSceneBundleDataContainer.GetData(list);
		if (data == null)
		{
			return new List<ChangeHeroInfo>(addedHeroChangeInfoList.Values);
		}
		bool reserveWindow = false;
		Action onCloseAction = null;
		int[] bundleHeroIndices = data.BundleHeroIndices;
		foreach (int key in bundleHeroIndices)
		{
			reserveWindow = addedHeroChangeInfoList[key].ReserveWindow;
			onCloseAction = addedHeroChangeInfoList[key].OnCloseAction;
			addedHeroChangeInfoList.Remove(key);
		}
		ChangeHeroInfo changeHeroInfo = new ChangeHeroInfo();
		changeHeroInfo.ChangeType = HeroChangeType.AddedHeros;
		changeHeroInfo.AddValue = data.Index;
		changeHeroInfo.NewValue = data.Index;
		changeHeroInfo.ReserveWindow = reserveWindow;
		changeHeroInfo.OnCloseAction = onCloseAction;
		addedHeroChangeInfoList[data.BundleHeroIndices[0]] = changeHeroInfo;
		return new List<ChangeHeroInfo>(addedHeroChangeInfoList.Values);
	}

	private void AnnounceToObserver(List<ChangeHeroInfo> announceList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(announceList); i++)
		{
			_observerManager.AnnounceToObservers(announceList[i]);
		}
	}

	private HeroChangeType GetChangeType(HeroInfo heroInfo)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroInfo.HeroIndex);
		if (cachedHeroInfo == null)
		{
			return HeroChangeType.AddedHero;
		}
		if (heroInfo.Star > cachedHeroInfo.Star)
		{
			return HeroChangeType.Star;
		}
		return HeroChangeType.None;
	}

	public void ApplyGrowthHeroInfo(HeroInfo heroInfo, Action onClose = null)
	{
		if (heroInfo != null)
		{
			HeroChangeType growthChangeType = GetGrowthChangeType(heroInfo);
			Add(new CachedHeroInfo(heroInfo, GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager, (int heroIndex, int accessoryIndex) => GetPositionInfo(heroIndex, accessoryIndex)));
			if (growthChangeType != HeroChangeType.None)
			{
				ResetAwakeMaterialChallenge(heroInfo.HeroIndex);
				ChangeHeroInfo changedInfo = new ChangeHeroInfo
				{
					ChangeType = growthChangeType,
					HeroIndex = heroInfo.HeroIndex,
					AddValue = heroInfo.HeroIndex,
					NewValue = heroInfo.HeroIndex,
					ReserveWindow = false,
					OnCloseAction = onClose
				};
				_observerManager.AnnounceToObservers(changedInfo);
			}
		}
	}

	public void ApplyAddOrGrowthHeroInfos(List<HeroInfo> heroInfos, Action<bool> onClose = null)
	{
		List<ChangeHeroInfo> list = null;
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
		{
			HeroInfo heroInfo = heroInfos[i];
			if (heroInfo == null)
			{
				continue;
			}
			HeroChangeType growthChangeType = GetGrowthChangeType(heroInfo, isAddAble: true);
			if (growthChangeType != HeroChangeType.None)
			{
				ChangeHeroInfo changeHeroInfo = new ChangeHeroInfo
				{
					ChangeType = growthChangeType,
					AddValue = heroInfo.HeroIndex,
					NewValue = heroInfo.HeroIndex,
					ReserveWindow = false
				};
				if (growthChangeType == HeroChangeType.AddedHero)
				{
					flag = true;
					changeHeroInfo.ReserveWindow = true;
					_AddedHeroIndices.Add(heroInfo.HeroIndex);
					_ = (object)null;
					_ = 19;
					_ = 1;
					_ = 0;
					_ = new object[2]
					{
						heroInfo.HeroIndex,
						Count()
					};
				}
				else
				{
					changeHeroInfo.HeroIndex = heroInfo.HeroIndex;
				}
				Add(new CachedHeroInfo(heroInfo, GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager, (int heroIndex, int accessoryIndex) => GetPositionInfo(heroIndex, accessoryIndex)));
				ResetAwakeMaterialChallenge(heroInfo.HeroIndex);
				if (list == null)
				{
					list = new List<ChangeHeroInfo>();
				}
				list.Add(changeHeroInfo);
			}
		}
		if (flag)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMilestonePopup(immediately: true, null);
		}
		list.Sort(delegate(ChangeHeroInfo lhs, ChangeHeroInfo rhs)
		{
			if (lhs.ChangeType == HeroChangeType.AddedHero)
			{
				return -1;
			}
			if (rhs.ChangeType == HeroChangeType.AddedHero)
			{
				return 1;
			}
			if (lhs.ChangeType == HeroChangeType.GrowthStar)
			{
				return -1;
			}
			if (rhs.ChangeType == HeroChangeType.GrowthStar)
			{
				return 1;
			}
			if (lhs.ChangeType == HeroChangeType.Transcended)
			{
				return -1;
			}
			return (rhs.ChangeType == HeroChangeType.Transcended) ? 1 : 0;
		});
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			Vespa_Generic_IList_Extension_Method.GetLast(list).OnCloseAction = delegate
			{
				Vespa_System_Action_Extension_Method.fire(onClose, i_argA: true);
			};
		}
		AnnounceToObserver(list);
	}

	private HeroChangeType GetGrowthChangeType(HeroInfo heroInfo, bool isAddAble = false)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroInfo.HeroIndex);
		if (cachedHeroInfo == null)
		{
			if (!isAddAble)
			{
				return HeroChangeType.None;
			}
			return HeroChangeType.AddedHero;
		}
		if (cachedHeroInfo.Level >= heroInfo.Level && cachedHeroInfo.Star >= heroInfo.Star && cachedHeroInfo.Transcended >= heroInfo.Transcended)
		{
			return HeroChangeType.AlreadyGrowth;
		}
		if (heroInfo.IsMaxStar() && (heroInfo.Transcended != cachedHeroInfo.Transcended || heroInfo.Level > cachedHeroInfo.Level))
		{
			return HeroChangeType.GrowthTranscended;
		}
		if (heroInfo.Star > cachedHeroInfo.Star || heroInfo.Level > cachedHeroInfo.Level)
		{
			return HeroChangeType.GrowthStar;
		}
		return HeroChangeType.None;
	}

	public void Set(HeroInfo[] heroInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
		{
			CachedHeroInfo value = new CachedHeroInfo(heroInfos[i], GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager, (int heroIndex, int accessoryIndex) => GetPositionInfo(heroIndex, accessoryIndex));
			Add(value);
		}
	}

	private PositionInfo GetPositionInfo(int heroIndex, int accessoryIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager == null)
		{
			return null;
		}
		PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo = GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(heroIndex, accessoryIndex));
		if (playerAccessoryCostumeInfo == null)
		{
			return null;
		}
		return PositionInfo.Create().TryAssign(playerAccessoryCostumeInfo.PositionInfo);
	}

	public int GetAddedHeroCount()
	{
		return _AddedHeroIndices.Count;
	}

	public bool InAddedHeroList(int HeroIndex)
	{
		return _AddedHeroIndices.Contains(HeroIndex);
	}

	public void ClearAddedHeroList()
	{
		_AddedHeroIndices.Clear();
		_observerManager.AnnounceToObservers(new ChangeHeroInfo
		{
			ChangeType = HeroChangeType.ClearedAddHero
		});
	}

	public List<CachedHeroInfo> GetHeroesByClass(CreatureTagType TagType)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		IEnumerator<CachedHeroInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			int heroIndex = enumerator.Current.HeroIndex;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			if (data != null && data.TagType == TagType)
			{
				list.Add(enumerator.Current);
			}
		}
		return list;
	}

	public List<CachedHeroInfo> GetAwakeChallengeableHeroes(CreatureTagType TagType)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		IEnumerator<CachedHeroInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			sbyte star = enumerator.Current.Star;
			int heroIndex = enumerator.Current.HeroIndex;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			HeroAwakeData data2 = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroIndex);
			if (data == null || data2 == null || (TagType != CreatureTagType.None && data.TagType != TagType))
			{
				continue;
			}
			string awakeMaterialItemCode = data2.GetAwakeMaterialItemCode(star);
			if (!String_Extension_Method.IsNullOrEmpty(awakeMaterialItemCode))
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(awakeMaterialItemCode);
				if (dataByCode != null && Helper.GetItemCount(dataByCode.Index) <= 0)
				{
					list.Add(enumerator.Current);
				}
			}
		}
		return list;
	}

	public List<CachedHeroInfo> GetTranscendChallengeableHeroes(CreatureTagType TagType)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		IEnumerator<CachedHeroInfo> enumerator = GetEnumerator();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		while (enumerator.MoveNext())
		{
			if (!IsMaxStar(enumerator.Current) || enumerator.Current.Transcended >= valueInt)
			{
				continue;
			}
			int heroIndex = enumerator.Current.HeroIndex;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
			HeroAwakeData data2 = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroIndex);
			if (data == null || data2 == null || (TagType != CreatureTagType.None && data.TagType != TagType))
			{
				continue;
			}
			string transcendMaterialItemCode = data2.GetTranscendMaterialItemCode(enumerator.Current.Transcended);
			if (!String_Extension_Method.IsNullOrEmpty(transcendMaterialItemCode))
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(transcendMaterialItemCode);
				if (dataByCode != null && Helper.GetItemCount(dataByCode.Index) <= 0)
				{
					list.Add(enumerator.Current);
				}
			}
		}
		return list;
	}

	public static bool IsMaxStar(CachedHeroInfo heroInfo)
	{
		return heroInfo?.IsMaxStar() ?? false;
	}

	public List<CachedHeroInfo> GetPurifiableHeroes(bool bTranscended = false)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		IEnumerator<CachedHeroInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!bTranscended && CanPurify(enumerator.Current))
			{
				list.Add(enumerator.Current);
			}
			else if (bTranscended && CanTranscendedPurify(enumerator.Current))
			{
				list.Add(enumerator.Current);
			}
		}
		return list;
	}

	public static bool CanPurify(CachedHeroInfo heroInfo)
	{
		sbyte star = heroInfo.Star;
		int heroIndex = heroInfo.HeroIndex;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		HeroAwakeData data = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return false;
		}
		if (star == valueInt)
		{
			return false;
		}
		if (Helper.GetAccountItemCount(data.GetAwakeMaterialItemCode(star)) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool CanTranscendedPurify(CachedHeroInfo heroInfo)
	{
		sbyte star = heroInfo.Star;
		int heroIndex = heroInfo.HeroIndex;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		HeroAwakeData data = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return false;
		}
		if (star < valueInt)
		{
			return false;
		}
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (heroInfo.Transcended >= valueInt2)
		{
			return false;
		}
		if (Helper.GetAccountItemCount(data.GetTranscendMaterialItemCode(heroInfo.Transcended)) == 0)
		{
			return false;
		}
		return true;
	}

	public void ApplyHeroPurified(HeroPurifyResultInfo result)
	{
		CachedHeroInfo cachedHeroInfo = Get(result.HeroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.Purified = result.NewValue;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.Purified,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = result.NewValue,
				AddValue = result.AddValue
			});
		}
	}

	public void ApplyHeroPurified(IList<HeroPurifyResultInfo> results)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(results); i++)
		{
			ApplyHeroPurified(results[i]);
		}
	}

	public void SetAwakeMaterialChallenge(int heroIndex, sbyte value)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.EnterDungeon = value;
		}
		_observerManager.AnnounceToObservers(new ChangeHeroInfo
		{
			ChangeType = HeroChangeType.EnterTrialDungeon,
			HeroIndex = cachedHeroInfo.HeroIndex
		});
	}

	public void ResetAwakeMaterialChallenge(int HeroIndex)
	{
		CachedHeroInfo cachedHeroInfo = Get(HeroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.EnterDungeon = 0;
		}
	}

	public void SetStar(int HeroIndex, int Star)
	{
		CachedHeroInfo cachedHeroInfo = Get(HeroIndex);
		if (cachedHeroInfo != null && Star != cachedHeroInfo.Star && Star > cachedHeroInfo.Star)
		{
			cachedHeroInfo.Star = (sbyte)Star;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.Star,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = Star
			});
		}
	}

	public void SetTranscended(int heroIndex, int transcended)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && transcended != cachedHeroInfo.Transcended)
		{
			cachedHeroInfo.Transcended = (sbyte)transcended;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.Transcended,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = transcended
			});
		}
	}

	public void SetSkillExtend(int heroIndex, int skillSlotIndex, int skillExtend)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && cachedHeroInfo.GetSkillExtend(skillSlotIndex) != skillExtend)
		{
			cachedHeroInfo.SetSkillExtend(skillSlotIndex, skillExtend);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.SkillExtended,
				HeroIndex = cachedHeroInfo.HeroIndex,
				SkillSlotIndex = skillSlotIndex,
				NewValue = skillExtend
			});
		}
	}

	public void SetSkillLevel(int heroIndex, int skillSlotIndex, int skillLevel)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null && cachedHeroInfo.GetSkillLevel(skillSlotIndex, isUserHero: true) != skillLevel)
		{
			cachedHeroInfo.SetSkillLevel(skillSlotIndex, skillLevel);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.SkillLevelup,
				HeroIndex = cachedHeroInfo.HeroIndex,
				SkillSlotIndex = skillSlotIndex,
				NewValue = skillLevel
			});
		}
	}

	public void SetTranscendSkill(int page, int heroIndex, string skillPage)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.SetTranscendSkillPageStringArray(page, skillPage);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.TranscendSkillSelected,
				HeroIndex = cachedHeroInfo.HeroIndex
			});
		}
	}

	public void SetTranscendSkill(int page, int heroIndex, int transcended, int skillSlotIndex, string skillPage)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.SetTranscendSkillSlotIndex(page, transcended, skillSlotIndex, skillPage);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.TranscendSkillSelected,
				HeroIndex = cachedHeroInfo.HeroIndex,
				Transcended = transcended,
				SkillSlotIndex = skillSlotIndex
			});
		}
	}

	public void ResetAllTranscendSkill(int heroIndex, int page)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.ResetAllTranscendSkillSlotIndex(page);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.ResetAllTranscendSkill,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = page
			});
		}
	}

	public void AddTranscendSkillPage(int heroIndex, int page)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.AddTranscendSkillPage(page, "", forceAdd: true);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.AddTranscendSkillPage,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = page
			});
		}
	}

	public void SelectTranscendSkillPage(int heroIndex, int page)
	{
		Get(heroIndex)?.SetTranscendSkillPage(page);
	}

	public void SetCostumeIndex(int heroIndex, int costumeIndex)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.CostumeIndex = costumeIndex;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.Costume,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = costumeIndex
			});
		}
	}

	public void SetHairCostumeIndex(int heroIndex, int hairCostumeIndex)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.HairCostumeIndex = hairCostumeIndex;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HairCostume,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = hairCostumeIndex
			});
		}
	}

	public void SetWeaponCostumeIndex(int heroIndex, int WeaponCostumeIndex)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.WeaponCostumeIndex = WeaponCostumeIndex;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.WeaponCostume,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = WeaponCostumeIndex,
				HideUniqueWeapon = cachedHeroInfo.HideUniqueWeapon
			});
		}
	}

	public void SetAccessoryCostume(int heroIndex, AccessoryPartType partType, int accessoryIndex, PositionInfo positionInfo)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.SetAccessoryCostume(partType, accessoryIndex, positionInfo);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.AccessoryCostume,
				HeroIndex = cachedHeroInfo.HeroIndex,
				PartIndex = (int)partType,
				NewValue = accessoryIndex
			});
		}
	}

	public void SetHideUniqueWeapon(int heroIndex, byte hideUniqueWeapon)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.HideUniqueWeapon = hideUniqueWeapon;
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.HideUniqueWeapon,
				HeroIndex = cachedHeroInfo.HeroIndex,
				NewValue = hideUniqueWeapon
			});
		}
	}

	public void SetEquipSlotOpened(int heroIndex, int partIndex)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.SetEquipSlotOpened(partIndex);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.OpenedEquipSlot,
				HeroIndex = heroIndex,
				NewValue = partIndex
			});
		}
	}

	public List<CachedHeroInfo> GetMaxTranscendHeroList()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		IEnumerator<CachedHeroInfo> enumerator = GetEnumerator();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Transcended == valueInt)
			{
				_ = enumerator.Current.HeroIndex;
				list.Add(enumerator.Current);
			}
		}
		return list;
	}

	public void SetExtendTranscendPoint(int heroIndex, int extendTranscendPoint)
	{
		CachedHeroInfo cachedHeroInfo = Get(heroIndex);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.SetExtendTranscendPoint(extendTranscendPoint);
			_observerManager.AnnounceToObservers(new ChangeHeroInfo
			{
				ChangeType = HeroChangeType.ExtendTranscendPoint,
				HeroIndex = heroIndex,
				NewValue = extendTranscendPoint
			});
		}
	}
}
