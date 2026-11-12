using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

namespace NGame2.NGuild;

public class GuildRaidBootyEquipItemManager : NShared.GuildRaidBootyEquipItemManager
{
	public enum EChangeType
	{
		Added,
		Removed
	}

	public class ChangeItemInfo
	{
		public EChangeType ChangeType;

		public GuildRaidBootyEquipItemInfo EquipItemInfo;
	}

	private float _lastUpdateTime;

	private HashSet<long> _NewAddedItemIdSet = new HashSet<long>();

	private UserObserverManager<ChangeItemInfo> _observerManager;

	private List<long> _DestroyedItemIds = new List<long>();

	public IUserObserverManager ObserverManager => _observerManager;

	public GuildRaidBootyEquipItemManager()
	{
		_observerManager = new UserObserverManager<ChangeItemInfo>();
	}

	public void UpdateItemDestroyTime()
	{
		if (Count() == 0 || Time.time - _lastUpdateTime < 1f)
		{
			return;
		}
		_lastUpdateTime = Time.time;
		using (IEnumerator<GuildRaidBootyEquipItemInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				GuildRaidBootyEquipItemInfo current = enumerator.Current;
				if (RaidHelper.BootyItemDestroyed(current.CreatedTime))
				{
					_DestroyedItemIds.Add(current.Id);
				}
			}
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_DestroyedItemIds); i++)
		{
			long itemId = _DestroyedItemIds[i];
			RemoveItem(itemId);
		}
		_DestroyedItemIds.Clear();
	}

	public void Apply(GuildRaidBootyEquipItemInfo[] equipItemInfos)
	{
		Set(equipItemInfos);
		if (equipItemInfos != null)
		{
			foreach (GuildRaidBootyEquipItemInfo guildRaidBootyEquipItemInfo in equipItemInfos)
			{
				_NewAddedItemIdSet.Add(guildRaidBootyEquipItemInfo.Id);
				AnnounceToObservers(new ChangeItemInfo
				{
					ChangeType = EChangeType.Added,
					EquipItemInfo = guildRaidBootyEquipItemInfo
				});
			}
		}
	}

	public void RemoveItem(long ItemId)
	{
		GuildRaidBootyEquipItemInfo guildRaidBootyEquipItemInfo = Get(ItemId);
		if (guildRaidBootyEquipItemInfo != null)
		{
			RemoveNew(ItemId);
			Remove(ItemId);
			AnnounceToObservers(new ChangeItemInfo
			{
				ChangeType = EChangeType.Removed,
				EquipItemInfo = guildRaidBootyEquipItemInfo
			});
		}
	}

	public bool IsNewItem(long ItemId)
	{
		return _NewAddedItemIdSet.Contains(ItemId);
	}

	private void AnnounceToObservers(ChangeItemInfo info)
	{
		if (_observerManager != null)
		{
			_observerManager.AnnounceToObservers(info);
		}
	}

	public void RemoveNew(long ItemId)
	{
		_NewAddedItemIdSet.Remove(ItemId);
	}

	public bool AddedItemExists()
	{
		return __Extension_Method_ICollection__.COUNT(_NewAddedItemIdSet) > 0;
	}

	public bool AddedItemExistsByPartType(EquipPartType partType)
	{
		HashSet<long>.Enumerator enumerator = _NewAddedItemIdSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			long current = enumerator.Current;
			GuildRaidBootyEquipItemInfo guildRaidBootyEquipItemInfo = Get(current);
			if (guildRaidBootyEquipItemInfo != null)
			{
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(guildRaidBootyEquipItemInfo.ItemIndex);
				if (data != null && data.SubData.PartType == partType)
				{
					return true;
				}
			}
		}
		return false;
	}
}
