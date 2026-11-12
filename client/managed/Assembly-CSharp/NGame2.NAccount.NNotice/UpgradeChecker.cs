using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount.NNotice;

public class UpgradeChecker : ISoulWeaponChecker, IChecker, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserHeroManager.ChangeHeroInfo>, IUserObserver<UserEquipItemManager.EquipStatInfo>, IUserObserver<UserSoulWeaponManager.SoulWeaponChangedInfo>, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>
{
	private Dictionary<int, UpgradeCheckData> _dic = new Dictionary<int, UpgradeCheckData>();

	private Dictionary<int, int> _dicEquipItemIndexLinkHeroIndex = new Dictionary<int, int>();

	private Dictionary<int, int> _dicItemIndexLinkHeroIndex = new Dictionary<int, int>();

	private HashSet<int> _hashEquipItemSlotIndices = new HashSet<int>();

	private ICheckerListener _listener;

	private void AnnounceToObservers(UpgradeCheckData data)
	{
		if (_listener != null && data != null)
		{
			SoulWeaponNoticeInfo noticeInfo = data.GetNoticeInfo();
			if (noticeInfo != null)
			{
				_listener.AnnounceToObservers(noticeInfo);
			}
		}
	}

	private void AddData(UpgradeCheckData data)
	{
		_dic.Add(data.HeroIndex, data);
		_dicEquipItemIndexLinkHeroIndex.Add(data.UniqueItemIndex, data.HeroIndex);
		_dicItemIndexLinkHeroIndex.Add(data.SoulStonItemIndex, data.HeroIndex);
	}

	private UpgradeCheckData SetHero(int heroIndex)
	{
		if (heroIndex == 0)
		{
			return null;
		}
		UpgradeCheckData dataByHeroIndex = GetDataByHeroIndex(heroIndex);
		dataByHeroIndex.SetHero();
		return dataByHeroIndex;
	}

	private UpgradeCheckData ChangeInventoryType(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		if (_hashEquipItemSlotIndices.Contains(equipItemInfo.SlotIndex))
		{
			UpgradeCheckData dataByEquipItemIndex = GetDataByEquipItemIndex(equipItemInfo.ItemIndex);
			if (dataByEquipItemIndex.SetEquipItem(equipItemInfo))
			{
				_hashEquipItemSlotIndices.Add(equipItemInfo.SlotIndex);
			}
			return dataByEquipItemIndex;
		}
		return null;
	}

	private UpgradeCheckData SetUniqueItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		EquipItemData equipItemData = equipItemInfo.EquipItemData;
		if (equipItemData == null || !equipItemData.IsOpenSoulWeapon)
		{
			return null;
		}
		UpgradeCheckData dataByEquipItemIndex = GetDataByEquipItemIndex(equipItemInfo.ItemIndex);
		if (dataByEquipItemIndex.SetEquipItem(equipItemInfo))
		{
			_hashEquipItemSlotIndices.Add(equipItemInfo.SlotIndex);
		}
		return dataByEquipItemIndex;
	}

	private UpgradeCheckData RemoveEquipItem(int slotIndex)
	{
		if (_hashEquipItemSlotIndices.Contains(slotIndex))
		{
			_hashEquipItemSlotIndices.Remove(slotIndex);
			foreach (KeyValuePair<int, UpgradeCheckData> item in _dic)
			{
				UpgradeCheckData value = item.Value;
				if (value != null && value.RemoveUniqueItem(slotIndex))
				{
					return value;
				}
			}
		}
		return null;
	}

	private UpgradeCheckData SetSoulStone(ItemInfo itemInfo)
	{
		if (itemInfo == null)
		{
			return null;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data == null)
		{
			return null;
		}
		if (data.Type != ItemType.SoulStone)
		{
			return null;
		}
		UpgradeCheckData dataByItemIndex = GetDataByItemIndex(itemInfo.ItemIndex);
		dataByItemIndex.SetSoulStone(itemInfo);
		return dataByItemIndex;
	}

	private UpgradeCheckData GetDataByHeroIndex(int heroIndex)
	{
		UpgradeCheckData value = null;
		if (!_dic.TryGetValue(heroIndex, out value))
		{
			value = new UpgradeCheckData(heroIndex, UpgradeCheckData.EConstructorType.Hero);
			AddData(value);
		}
		return value;
	}

	private UpgradeCheckData GetDataByEquipItemIndex(int equipItemIndex)
	{
		UpgradeCheckData upgradeCheckData = null;
		int value = 0;
		if (!_dicEquipItemIndexLinkHeroIndex.TryGetValue(equipItemIndex, out value))
		{
			upgradeCheckData = new UpgradeCheckData(equipItemIndex, UpgradeCheckData.EConstructorType.UniqueItem);
			AddData(upgradeCheckData);
		}
		else
		{
			upgradeCheckData = GetDataByHeroIndex(value);
		}
		return upgradeCheckData;
	}

	private UpgradeCheckData GetDataByItemIndex(int itemIndex)
	{
		UpgradeCheckData upgradeCheckData = null;
		int value = 0;
		if (!_dicItemIndexLinkHeroIndex.TryGetValue(itemIndex, out value))
		{
			upgradeCheckData = new UpgradeCheckData(itemIndex, UpgradeCheckData.EConstructorType.SoulStone);
			AddData(upgradeCheckData);
		}
		else
		{
			upgradeCheckData = GetDataByHeroIndex(value);
		}
		return upgradeCheckData;
	}

	void IChecker.Set(ICheckerListener listener)
	{
		if (!AccountManager.IsLogin())
		{
			VespaDebug.LogError("[SoulWeaponNoticeManager]InValid Set Timing ");
			return;
		}
		_listener = listener;
		AccountManager instance = GlobalAutoBehaviour<AccountManager>.instance;
		UserEquipItemManager equipItemManager = instance.EquipItemManager;
		UserHeroManager heroManager = instance.HeroManager;
		UserItemManager itemManager = instance.ItemManager;
		UserDispatchBattleInfoManager dispatchBattleInfoManager = instance.DispatchBattleInfoManager;
		if (heroManager != null)
		{
			foreach (CachedHeroInfo item in heroManager)
			{
				int heroIndex = item.HeroIndex;
				UpgradeCheckData upgradeCheckData = SetHero(heroIndex);
				if (dispatchBattleInfoManager != null)
				{
					bool dispatch = dispatchBattleInfoManager.GetByHeroIndex(heroIndex) != null;
					upgradeCheckData.SetDispatch(dispatch);
				}
			}
		}
		if (equipItemManager != null)
		{
			foreach (EquipItemInfo item2 in equipItemManager)
			{
				SetUniqueItem(item2);
			}
		}
		if (itemManager != null)
		{
			List<ItemInfo> itemsByType = instance.ItemManager.GetItemsByType(ItemType.SoulStone);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
			{
				SetSoulStone(itemsByType[i]);
			}
		}
		foreach (KeyValuePair<int, UpgradeCheckData> item3 in _dic)
		{
			item3.Value?.GetNoticeInfo();
		}
	}

	void IChecker.AddObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		}
	}

	void IChecker.RemoveObserver()
	{
		if (AccountManager.IsLogin())
		{
			GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		}
	}

	void IChecker.Close()
	{
		_dic.Clear();
		_dicEquipItemIndexLinkHeroIndex.Clear();
		_dicItemIndexLinkHeroIndex.Clear();
		_hashEquipItemSlotIndices.Clear();
		_listener = null;
	}

	private bool ValidNoticeType(ESoulWeaponNoticeType noticeType)
	{
		if (noticeType != ESoulWeaponNoticeType.Liberation)
		{
			return noticeType == ESoulWeaponNoticeType.Upgrade;
		}
		return true;
	}

	bool ISoulWeaponChecker.IsNoticeType(ESoulWeaponNoticeType noticeType)
	{
		return ValidNoticeType(noticeType);
	}

	bool ISoulWeaponChecker.AnyStateOn()
	{
		foreach (KeyValuePair<int, UpgradeCheckData> item in _dic)
		{
			UpgradeCheckData value = item.Value;
			if (value != null && value.GetState().Item2)
			{
				return true;
			}
		}
		return false;
	}

	bool ISoulWeaponChecker.GetNoticeTypeState(ESoulWeaponNoticeType noticeType)
	{
		if (!ValidNoticeType(noticeType))
		{
			return false;
		}
		foreach (KeyValuePair<int, UpgradeCheckData> item in _dic)
		{
			UpgradeCheckData value = item.Value;
			if (value != null)
			{
				Tuple<ESoulWeaponNoticeType, bool> state = value.GetState();
				if (state.Item1 == noticeType && state.Item2)
				{
					return true;
				}
			}
		}
		return false;
	}

	bool ISoulWeaponChecker.GetNoticeTargetState(int equipItemSlotIndex)
	{
		if (_hashEquipItemSlotIndices.Contains(equipItemSlotIndex))
		{
			EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
			if (equipItemInfo == null)
			{
				VespaDebug.LogError("[SoulWeaponNoticeManager] Data Matching Error");
				return false;
			}
			if (!equipItemInfo.IsUniqueWeapon)
			{
				VespaDebug.LogError("[SoulWeaponNoticeManager] Not UniqueWeapon ItemIndex : " + equipItemInfo.ItemIndex);
				return false;
			}
			EquipItemData equipItemData = equipItemInfo.EquipItemData;
			if (equipItemData == null || !equipItemData.IsOpenSoulWeapon)
			{
				VespaDebug.LogError("[SoulWeaponNoticeManager] EquipItemData Invalid ItemIndex : " + equipItemInfo.ItemIndex);
				return false;
			}
			return GetDataByEquipItemIndex(equipItemInfo.ItemIndex).GetState().Item2;
		}
		return false;
	}

	bool IUserObserver.IsGlobal()
	{
		return true;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		UpgradeCheckData data = SetSoulStone(changedInfo);
		AnnounceToObservers(data);
	}

	void IUserObserver<UserHeroManager.ChangeHeroInfo>.OnChanged(UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == UserHeroManager.HeroChangeType.AddedHero)
		{
			UpgradeCheckData data = SetHero(changedInfo.AddValue);
			AnnounceToObservers(data);
		}
	}

	void IUserObserver<UserEquipItemManager.EquipStatInfo>.OnChanged(UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo.StatType == UserEquipItemManager.EquipStatType.Add)
		{
			UpgradeCheckData data = SetUniqueItem(changedInfo.EquipItemInfo);
			AnnounceToObservers(data);
		}
		else if (changedInfo.StatType == UserEquipItemManager.EquipStatType.Remove)
		{
			UpgradeCheckData data2 = RemoveEquipItem(changedInfo.SlotIndex);
			AnnounceToObservers(data2);
		}
		else if (changedInfo.StatType == UserEquipItemManager.EquipStatType.ChangeInventoryType)
		{
			UpgradeCheckData data3 = ChangeInventoryType(changedInfo.EquipItemInfo);
			AnnounceToObservers(data3);
		}
	}

	void IUserObserver<UserSoulWeaponManager.SoulWeaponChangedInfo>.OnChanged(UserSoulWeaponManager.SoulWeaponChangedInfo changedInfo)
	{
		if (changedInfo.ChangedType == UserSoulWeaponManager.ESoulWeaponChangedType.EquipItemSlotIndex || changedInfo.ChangedType == UserSoulWeaponManager.ESoulWeaponChangedType.ReinforceLevel || changedInfo.ChangedType == UserSoulWeaponManager.ESoulWeaponChangedType.Grade || changedInfo.ChangedType == UserSoulWeaponManager.ESoulWeaponChangedType.Add)
		{
			SoulWeaponInfo soulWeaponInfo = changedInfo.SoulWeaponInfo;
			UpgradeCheckData dataByEquipItemIndex = GetDataByEquipItemIndex(soulWeaponInfo.ItemIndex);
			dataByEquipItemIndex.SetSoulWeaponInfo(soulWeaponInfo);
			AnnounceToObservers(dataByEquipItemIndex);
		}
		else if (changedInfo.ChangedType == UserSoulWeaponManager.ESoulWeaponChangedType.Remove)
		{
			UpgradeCheckData dataByEquipItemIndex2 = GetDataByEquipItemIndex(changedInfo.ItemIndex);
			dataByEquipItemIndex2.SetSoulWeaponInfo(null);
			AnnounceToObservers(dataByEquipItemIndex2);
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo changedInfo)
	{
		if (changedInfo.Reason != UserDispatchBattleInfoManager.Observer.BroadCastReason.OnAdd && changedInfo.Reason != UserDispatchBattleInfoManager.Observer.BroadCastReason.OnRemove)
		{
			return;
		}
		bool dispatch = changedInfo.Reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnAdd;
		List<int> heroIndexList = changedInfo.Info.HeroIndexList;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroIndexList); i++)
		{
			int num = heroIndexList[i];
			if (num != 0)
			{
				UpgradeCheckData dataByHeroIndex = GetDataByHeroIndex(num);
				dataByHeroIndex.SetDispatch(dispatch);
				AnnounceToObservers(dataByHeroIndex);
			}
		}
	}
}
