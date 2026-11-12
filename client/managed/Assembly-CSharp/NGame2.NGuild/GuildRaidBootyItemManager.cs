using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using UnityEngine;

namespace NGame2.NGuild;

public class GuildRaidBootyItemManager : NShared.GuildRaidBootyItemManager
{
	public enum EChangeType
	{
		Added,
		Removed
	}

	public class ChangeItemInfo
	{
		public EChangeType ChangeType;

		public GuildRaidBootyItemInfo ItemInfo;
	}

	private HashSet<long> _NewAddedItemIdSet = new HashSet<long>();

	private UserObserverManager<ChangeItemInfo> _observerManager;

	private float _lastUpdateTime;

	private List<long> _DestroyedItemIds = new List<long>();

	public IUserObserverManager ObserverManager => _observerManager;

	public GuildRaidBootyItemManager()
	{
		_observerManager = new UserObserverManager<ChangeItemInfo>();
	}

	public void Apply(GuildRaidBootyItemInfo[] ItemInfos)
	{
		Set(ItemInfos);
		if (ItemInfos != null)
		{
			foreach (GuildRaidBootyItemInfo guildRaidBootyItemInfo in ItemInfos)
			{
				_NewAddedItemIdSet.Add(guildRaidBootyItemInfo.Id);
				AnnounceToObservers(new ChangeItemInfo
				{
					ChangeType = EChangeType.Added,
					ItemInfo = guildRaidBootyItemInfo
				});
			}
		}
	}

	public void UpdateItemDestroyTime()
	{
		if (Count() == 0 || Time.time - _lastUpdateTime < 1f)
		{
			return;
		}
		_lastUpdateTime = Time.time;
		using (IEnumerator<GuildRaidBootyItemInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				GuildRaidBootyItemInfo current = enumerator.Current;
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

	public void RemoveItem(long ItemId)
	{
		GuildRaidBootyItemInfo guildRaidBootyItemInfo = Get(ItemId);
		if (guildRaidBootyItemInfo != null)
		{
			RemoveNew(ItemId);
			Remove(ItemId);
			AnnounceToObservers(new ChangeItemInfo
			{
				ChangeType = EChangeType.Removed,
				ItemInfo = guildRaidBootyItemInfo
			});
		}
	}

	public List<GuildRaidBootyItemInfo> GetListByItemTypes(ItemType[] itemTypes)
	{
		List<GuildRaidBootyItemInfo> list = ToList();
		List<GuildRaidBootyItemInfo> list2 = new List<GuildRaidBootyItemInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			int itemIndex = list[i].ItemIndex;
			ItemData ItemData = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (ItemData != null && Array.FindIndex(itemTypes, (ItemType type) => type == ItemData.Type) != -1)
			{
				list2.Add(list[i]);
			}
		}
		return list2;
	}

	public bool AddedItemExistsByItemType(ItemType itemType)
	{
		HashSet<long>.Enumerator enumerator = _NewAddedItemIdSet.GetEnumerator();
		while (enumerator.MoveNext())
		{
			long current = enumerator.Current;
			GuildRaidBootyItemInfo guildRaidBootyItemInfo = Get(current);
			if (guildRaidBootyItemInfo != null)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(guildRaidBootyItemInfo.ItemIndex);
				if (data != null && data.Type == itemType)
				{
					return true;
				}
			}
		}
		return false;
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
}
