using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class HeroNotifyContainer : SceneUpdateBehaviour<HeroNotifyContainer>, HeroPresetStorageWebManager.ILoadPresetListener, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver, IUserObserver<ItemInfo>, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>
{
	private class HeroNotifyCache
	{
		public List<EquipPartType> RuneAddableParts = new List<EquipPartType>();

		public bool TrialChallengable { get; set; }

		public bool Purifiable { get; set; }

		public bool CanSkillExtend { get; set; }

		public bool CanLimitBreak { get; set; }

		public bool CanEquipRune()
		{
			if (RuneAddableParts == null)
			{
				return false;
			}
			return RuneAddableParts.Count > 0;
		}
	}

	private class DirtyNotify
	{
		public List<HeroNotifyComponent.ENotifyInfo> DirtyTypeList = new List<HeroNotifyComponent.ENotifyInfo>();

		public void Release()
		{
			DirtyTypeList.Clear();
		}
	}

	private class PartTypeComparer : IEqualityComparer<EquipPartType>
	{
		public bool Equals(EquipPartType x, EquipPartType y)
		{
			return x == y;
		}

		public int GetHashCode(EquipPartType obj)
		{
			return (int)obj;
		}
	}

	private Dictionary<int, HeroNotifyCache> NotifyCacheDic = new Dictionary<int, HeroNotifyCache>(15);

	private Dictionary<int, DirtyNotify> DirtyNotifyDic = new Dictionary<int, DirtyNotify>(15);

	private bool bDirtyRune;

	private bool _isSkipEquipCheck;

	private Dictionary<EquipPartType, byte> BestRuneGradeForPart = new Dictionary<EquipPartType, byte>(new PartTypeComparer());

	private Dictionary<string, byte> BestRuneGradeForGroup = new Dictionary<string, byte>();

	private List<HeroNotifyComponent.ENotifyInfo> ListNotifyInfos = new List<HeroNotifyComponent.ENotifyInfo>(3);

	private Queue<DirtyNotify> UnusedDirtyNotify = new Queue<DirtyNotify>(5);

	private const int MAX_QUEUE_COUNT = 150;

	private DateTime _lastDirtyUpdateTime = DateTime.UtcNow;

	private bool IsContainPartType(List<EquipPartType> listPartType, EquipPartType partType)
	{
		for (int i = 0; i < listPartType.Count; i++)
		{
			if (listPartType[i] == partType)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsContainNotify(List<HeroNotifyComponent.ENotifyInfo> listNotify, HeroNotifyComponent.ENotifyInfo notify)
	{
		for (int i = 0; i < listNotify.Count; i++)
		{
			if (listNotify[i] == notify)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SingletonAuto<HeroPresetStorageWebManager>.instance.RemoveLoadPresetListener(this);
		while (UnusedDirtyNotify.Count > 0)
		{
			UnusedDirtyNotify.Dequeue().Release();
		}
	}

	private void PushDirtyNotify(DirtyNotify PushedDirtyNotify)
	{
		PushedDirtyNotify.Release();
		if (UnusedDirtyNotify.Count < 150)
		{
			UnusedDirtyNotify.Enqueue(PushedDirtyNotify);
		}
	}

	private DirtyNotify PopDirtyNotify()
	{
		if (UnusedDirtyNotify.Count > 0)
		{
			return UnusedDirtyNotify.Dequeue();
		}
		return new DirtyNotify();
	}

	private bool CheckTrialChallengable(int heroIndex)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return false;
		}
		return TrialChallengeHeroSelect.CanHeroTrialChallenge(heroInfo);
	}

	private bool CheckPurifiable(int heroIndex)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return false;
		}
		if (NGame2.NAccount.UserHeroManager.IsMaxStar(heroInfo))
		{
			return NGame2.NAccount.UserHeroManager.CanTranscendedPurify(heroInfo);
		}
		return NGame2.NAccount.UserHeroManager.CanPurify(heroInfo);
	}

	private bool CheckCanSkillExtend(int heroIndex)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return false;
		}
		return Helper.CanAnySkillExtend(heroInfo);
	}

	private bool CheckCanLimitBreak(int heroIndex)
	{
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return false;
		}
		if (!heroInfo.CanIncLimitBreakLevel())
		{
			return false;
		}
		if (!heroInfo.IsMaxStar())
		{
			return false;
		}
		if (heroInfo.Transcended < GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend"))
		{
			return false;
		}
		HeroLimitBreakData data = GameRoot.Instance.HeroLimitBreakDataContainer.GetData(new HeroLimitBreakDataKey(heroInfo.HeroIndex, heroInfo.LimitBreakLevel + 1));
		if (data == null)
		{
			return false;
		}
		List<Tuple<string, int>> materialItems = data.GetMaterialItems();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(materialItems); i++)
		{
			string item = materialItems[i].Item1;
			int item2 = materialItems[i].Item2;
			if (Helper.GetItemCount(GameRoot.Instance.ItemDataContainer.GetDataByCode(item).Index) < item2)
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateBestRuneDic()
	{
		BestRuneGradeForPart.Clear();
		BestRuneGradeForGroup.Clear();
		foreach (ItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Rune))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			EquipPartType[] partTypes = data.GetSubData<RuneItemData>().PartTypes;
			foreach (EquipPartType key in partTypes)
			{
				if (BestRuneGradeForPart.ContainsKey(key))
				{
					if (BestRuneGradeForPart[key] < data.Grade)
					{
						BestRuneGradeForPart[key] = data.Grade;
					}
				}
				else
				{
					BestRuneGradeForPart.Add(key, data.Grade);
				}
			}
			string groupCode = data.GetSubData<RuneItemData>().GroupCode;
			if (BestRuneGradeForGroup.ContainsKey(groupCode))
			{
				if (BestRuneGradeForGroup[groupCode] < data.Grade)
				{
					BestRuneGradeForGroup[groupCode] = data.Grade;
				}
			}
			else
			{
				BestRuneGradeForGroup.Add(groupCode, data.Grade);
			}
		}
		bDirtyRune = false;
	}

	private byte GetRuneScore(EquipItemInfo equipItemInfo, int idx)
	{
		int num = idx switch
		{
			0 => equipItemInfo.RuneItemIndex1, 
			1 => equipItemInfo.RuneItemIndex2, 
			_ => equipItemInfo.RuneItemIndex3, 
		};
		if (num == 0)
		{
			return 0;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num);
		if (data == null)
		{
			return 0;
		}
		return GameRoot.Instance.ItemGradeDataContainer.GetData(data.Grade)?.Score ?? 0;
	}

	private string GetRuneGroupCode(EquipItemInfo equipItemInfo, int idx)
	{
		int num = idx switch
		{
			0 => equipItemInfo.RuneItemIndex1, 
			1 => equipItemInfo.RuneItemIndex2, 
			_ => equipItemInfo.RuneItemIndex3, 
		};
		if (num == 0)
		{
			return null;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num);
		if (data == null)
		{
			return null;
		}
		return GameRoot.Instance.RuneItemDataContainer.GetData(data.Code)?.GroupCode;
	}

	private void CheckEquipRuneExist(int heroIndex, List<EquipPartType> runeAddableParts)
	{
		runeAddableParts.Clear();
		CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
		if (heroInfo == null)
		{
			return;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			EquipItemInfo equipItemInfoByPartIndex = heroInfo.GetEquipItemInfoByPartIndex(allEquipSlotDatum.Index);
			if (equipItemInfoByPartIndex == null || equipItemInfoByPartIndex.RuneSlotCount == 0)
			{
				continue;
			}
			for (int i = 0; i < equipItemInfoByPartIndex.RuneSlotCount; i++)
			{
				byte runeScore = GetRuneScore(equipItemInfoByPartIndex, i);
				string runeGroupCode = GetRuneGroupCode(equipItemInfoByPartIndex, i);
				byte b = 0;
				b = (byte)((runeGroupCode == null) ? (BestRuneGradeForPart.ContainsKey(allEquipSlotDatum.EquipPartType) ? BestRuneGradeForPart[allEquipSlotDatum.EquipPartType] : 0) : (BestRuneGradeForGroup.ContainsKey(runeGroupCode) ? BestRuneGradeForGroup[runeGroupCode] : 0));
				if (b == 0)
				{
					continue;
				}
				ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(b);
				if (data != null && runeScore < data.Score)
				{
					if (!IsContainPartType(runeAddableParts, allEquipSlotDatum.EquipPartType))
					{
						runeAddableParts.Add(allEquipSlotDatum.EquipPartType);
					}
					break;
				}
			}
		}
	}

	private void AddHeroToCacheDic(int heroIndex)
	{
		HeroNotifyCache heroNotifyCache = new HeroNotifyCache();
		heroNotifyCache.Purifiable = CheckPurifiable(heroIndex);
		heroNotifyCache.TrialChallengable = CheckTrialChallengable(heroIndex);
		heroNotifyCache.CanSkillExtend = CheckCanSkillExtend(heroIndex);
		heroNotifyCache.CanLimitBreak = CheckCanLimitBreak(heroIndex);
		CheckEquipRuneExist(heroIndex, heroNotifyCache.RuneAddableParts);
		NotifyCacheDic.Add(heroIndex, heroNotifyCache);
	}

	public void Init()
	{
		NotifyCacheDic.Clear();
		DirtyNotifyDic.Clear();
		BestRuneGradeForPart.Clear();
		BestRuneGradeForGroup.Clear();
		ListNotifyInfos.Clear();
		UnusedDirtyNotify.Clear();
		UpdateBestRuneDic();
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
		{
			AddHeroToCacheDic(heroInfo.HeroIndex);
		}
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		SingletonAuto<HeroPresetStorageWebManager>.instance.AddLoadPresetListener(this);
		_isSkipEquipCheck = false;
	}

	public List<HeroNotifyComponent.ENotifyInfo> GetHeroNotifyInfoList(int heroIndex)
	{
		ListNotifyInfos.Clear();
		if (NotifyCacheDic.TryGetValue(heroIndex, out var value))
		{
			if (value.Purifiable)
			{
				ListNotifyInfos.Add(HeroNotifyComponent.ENotifyInfo.Purifiable);
			}
			else if (value.TrialChallengable)
			{
				ListNotifyInfos.Add(HeroNotifyComponent.ENotifyInfo.TrialChallengable);
			}
			if (value.CanSkillExtend)
			{
				ListNotifyInfos.Add(HeroNotifyComponent.ENotifyInfo.CanSkillExtend);
			}
			if (value.CanLimitBreak)
			{
				ListNotifyInfos.Add(HeroNotifyComponent.ENotifyInfo.CanLimitBreak);
			}
		}
		else
		{
			AddHeroToCacheDic(heroIndex);
		}
		return ListNotifyInfos;
	}

	public bool CheckRuneAddableForPart(int heroIndex, EquipPartType partType)
	{
		if (!NotifyCacheDic.ContainsKey(heroIndex))
		{
			AddHeroToCacheDic(heroIndex);
		}
		return IsContainPartType(NotifyCacheDic[heroIndex].RuneAddableParts, partType);
	}

	public bool GetAnyNotifyExist()
	{
		foreach (KeyValuePair<int, HeroNotifyCache> item in NotifyCacheDic)
		{
			if (item.Value.Purifiable || item.Value.TrialChallengable || item.Value.CanEquipRune())
			{
				return true;
			}
		}
		return false;
	}

	private void AddDirtyNotify(int heroIndex, HeroNotifyComponent.ENotifyInfo type)
	{
		if (DirtyNotifyDic.ContainsKey(heroIndex))
		{
			if (!IsContainNotify(DirtyNotifyDic[heroIndex].DirtyTypeList, type))
			{
				DirtyNotifyDic[heroIndex].DirtyTypeList.Add(type);
			}
		}
		else
		{
			DirtyNotify dirtyNotify = PopDirtyNotify();
			dirtyNotify.DirtyTypeList.Add(type);
			DirtyNotifyDic.Add(heroIndex, dirtyNotify);
		}
	}

	private void UpdateDirtyNotify(int heroIndex, DirtyNotify dirtyNotifyComp)
	{
		if (NotifyCacheDic.TryGetValue(heroIndex, out var value))
		{
			List<HeroNotifyComponent.ENotifyInfo> dirtyTypeList = dirtyNotifyComp.DirtyTypeList;
			for (int i = 0; i < dirtyTypeList.Count; i++)
			{
				if (dirtyTypeList[i] == HeroNotifyComponent.ENotifyInfo.Purifiable)
				{
					value.Purifiable = CheckPurifiable(heroIndex);
				}
				else if (dirtyTypeList[i] == HeroNotifyComponent.ENotifyInfo.TrialChallengable)
				{
					value.TrialChallengable = CheckTrialChallengable(heroIndex);
				}
				else if (dirtyTypeList[i] == HeroNotifyComponent.ENotifyInfo.CanSkillExtend)
				{
					value.CanSkillExtend = CheckCanSkillExtend(heroIndex);
				}
				else if (dirtyTypeList[i] == HeroNotifyComponent.ENotifyInfo.CanLimitBreak)
				{
					value.CanLimitBreak = CheckCanLimitBreak(heroIndex);
				}
			}
		}
		else
		{
			AddHeroToCacheDic(heroIndex);
		}
		dirtyNotifyComp.DirtyTypeList.Clear();
	}

	protected override void ChainUpdate()
	{
		if ((DateTime.UtcNow - _lastDirtyUpdateTime).TotalMilliseconds < 1000.0)
		{
			return;
		}
		_lastDirtyUpdateTime = DateTime.UtcNow;
		if (bDirtyRune)
		{
			UpdateBestRuneDic();
		}
		foreach (KeyValuePair<int, DirtyNotify> item in DirtyNotifyDic)
		{
			UpdateDirtyNotify(item.Key, item.Value);
			PushDirtyNotify(item.Value);
		}
		DirtyNotifyDic.Clear();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (!_isSkipEquipCheck)
		{
			EquipStatInfoChanged();
		}
	}

	private void EquipStatInfoChanged()
	{
		foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
		{
			NotifyCacheDic.ContainsKey(heroInfo.HeroIndex);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo info)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(info.ItemIndex);
		if (data == null)
		{
			return;
		}
		if (data.Type == ItemType.UpgradeBook)
		{
			foreach (CachedHeroInfo heroInfo in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
			{
				AddDirtyNotify(heroInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanSkillExtend);
			}
			return;
		}
		if (data.Type == ItemType.AwakeChallenge || data.Type == ItemType.AwakeMaterial)
		{
			foreach (CachedHeroInfo heroInfo2 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
			{
				AddDirtyNotify(heroInfo2.HeroIndex, HeroNotifyComponent.ENotifyInfo.Purifiable);
				AddDirtyNotify(heroInfo2.HeroIndex, HeroNotifyComponent.ENotifyInfo.TrialChallengable);
			}
			return;
		}
		if (data.Type != ItemType.HeroEnhanceMaterial)
		{
			return;
		}
		foreach (CachedHeroInfo heroInfo3 in GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: false))
		{
			AddDirtyNotify(heroInfo3.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanLimitBreak);
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		switch (changedInfo.ChangeType)
		{
		case NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup:
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanLimitBreak);
			break;
		case NGame2.NAccount.UserHeroManager.HeroChangeType.SkillExtended:
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanSkillExtend);
			break;
		case NGame2.NAccount.UserHeroManager.HeroChangeType.Purified:
		case NGame2.NAccount.UserHeroManager.HeroChangeType.Star:
		case NGame2.NAccount.UserHeroManager.HeroChangeType.Transcended:
		case NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthStar:
		case NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthTranscended:
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.Purifiable);
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.TrialChallengable);
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanLimitBreak);
			break;
		case NGame2.NAccount.UserHeroManager.HeroChangeType.AddedHero:
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.Purifiable);
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.TrialChallengable);
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanSkillExtend);
			AddDirtyNotify(changedInfo.HeroIndex, HeroNotifyComponent.ENotifyInfo.CanLimitBreak);
			break;
		}
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnBeforeLoadPreset()
	{
		_isSkipEquipCheck = true;
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnAfterLoadPreset()
	{
		_isSkipEquipCheck = false;
		EquipStatInfoChanged();
	}
}
