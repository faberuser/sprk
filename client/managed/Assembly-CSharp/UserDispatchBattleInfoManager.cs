using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NVespa;

public class UserDispatchBattleInfoManager : AccountAttrManager<int, WCL_DispatchBattleInfo>
{
	public class Observer
	{
		public enum BroadCastReason
		{
			OnAdd,
			OnUpdate,
			OnChangeState,
			OnRemove
		}

		public struct BroadCastInfo
		{
			private int _slotIndex;

			private WCL_DispatchBattleInfo _info;

			private BroadCastReason _reason;

			public int SlotIndex => _slotIndex;

			public WCL_DispatchBattleInfo Info => _info;

			public BroadCastReason Reason => _reason;

			public BroadCastInfo(int slotIndex, WCL_DispatchBattleInfo info, BroadCastReason reason)
			{
				_slotIndex = slotIndex;
				_info = info;
				_reason = reason;
			}
		}

		private UserObserverManager<BroadCastInfo> _observerManager;

		public UserObserverManager<BroadCastInfo> ObserverManager => _observerManager;

		public Observer()
		{
			_observerManager = new UserObserverManager<BroadCastInfo>();
		}
	}

	private Observer _observer = new Observer();

	private Dictionary<int, Dictionary<int, Dictionary<int, WCL_DispatchBattleInfo>>> _dungeonKeyToInfotable = new Dictionary<int, Dictionary<int, Dictionary<int, WCL_DispatchBattleInfo>>>();

	private Dictionary<DispatchType, List<WCL_DispatchBattleInfo>> _dispatchTypeExceptChapterContainListTable = new Dictionary<DispatchType, List<WCL_DispatchBattleInfo>>();

	public IUserObserverManager ObserverManager => _observer.ObserverManager;

	public UserDispatchBattleInfoManager()
	{
		_observer = new Observer();
	}

	public override void Set(WCL_DispatchBattleInfo[] newInfos)
	{
		if (Count() > 0)
		{
			HashSet<int> hashSet = new HashSet<int>();
			int i = 0;
			for (int num = __Extension_Method_ICollection__.COUNT(newInfos); i < num; i++)
			{
				WCL_DispatchBattleInfo wCL_DispatchBattleInfo = newInfos[i];
				if (wCL_DispatchBattleInfo != null)
				{
					hashSet.Add(wCL_DispatchBattleInfo.SlotIndex);
				}
			}
			WCL_DispatchBattleInfo[] values = GetValues();
			int j = 0;
			for (int num2 = __Extension_Method_ICollection__.COUNT(values); j < num2; j++)
			{
				WCL_DispatchBattleInfo wCL_DispatchBattleInfo2 = values[j];
				if (wCL_DispatchBattleInfo2 != null && !hashSet.Contains(wCL_DispatchBattleInfo2.SlotIndex))
				{
					Remove(wCL_DispatchBattleInfo2.SlotIndex);
				}
			}
		}
		base.Set(newInfos);
		SprkDispatchSelection.RefreshOpenLists();
	}

	public override bool Remove(WCL_DispatchBattleInfo info)
	{
		OnRemove(info);
		bool result = base.Remove(info);
		SprkDispatchSelection.RefreshOpenLists();
		return result;
	}

	public override bool Remove(int key)
	{
		OnRemove(Get(key));
		bool result = base.Remove(key);
		SprkDispatchSelection.RefreshOpenLists();
		return result;
	}

	public void UpdateInfo(int key, WCL_DispatchBattleInfo info)
	{
		OnUpdateInfo(key, info);
	}

	public WCL_DispatchBattleInfo Get(int chapterIndex, int dungeonIndex, int deckIndex)
	{
		if (!_dungeonKeyToInfotable.ContainsKey(chapterIndex))
		{
			return null;
		}
		if (!_dungeonKeyToInfotable[chapterIndex].ContainsKey(dungeonIndex))
		{
			return null;
		}
		if (!_dungeonKeyToInfotable[chapterIndex][dungeonIndex].ContainsKey(deckIndex))
		{
			return null;
		}
		return _dungeonKeyToInfotable[chapterIndex][dungeonIndex][deckIndex];
	}

	public WCL_DispatchBattleInfo GetByRaid(DispatchType dispatchType, int deckIndex)
	{
		if (!_dispatchTypeExceptChapterContainListTable.ContainsKey(dispatchType))
		{
			return null;
		}
		List<WCL_DispatchBattleInfo> list = _dispatchTypeExceptChapterContainListTable[dispatchType];
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(list); i < num; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = list[i];
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.DeckIndex == deckIndex)
			{
				return wCL_DispatchBattleInfo;
			}
		}
		return null;
	}

	public List<WCL_DispatchBattleInfo> GetListByChapterIndex(DispatchType dispatchType)
	{
		if (!_dispatchTypeExceptChapterContainListTable.ContainsKey(dispatchType))
		{
			return new List<WCL_DispatchBattleInfo>();
		}
		return _dispatchTypeExceptChapterContainListTable[dispatchType];
	}

	public WCL_DispatchBattleInfo GetByHeroIndex(int heroIndex)
	{
		IEnumerator<WCL_DispatchBattleInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			WCL_DispatchBattleInfo current = enumerator.Current;
			if (current != null && current.HeroIndexList.Contains(heroIndex))
			{
				return current;
			}
		}
		return null;
	}

	public bool IsCompleteDispatchBattle(WCL_DispatchBattleInfo exceptionInfo = null)
	{
		WCL_DispatchBattleInfo wCL_DispatchBattleInfo = null;
		WCL_DispatchBattleInfo[] values = GetValues();
		for (int i = 0; i < values.Length; i++)
		{
			wCL_DispatchBattleInfo = values[i];
			if (wCL_DispatchBattleInfo != null && (exceptionInfo == null || wCL_DispatchBattleInfo.SlotIndex != exceptionInfo.SlotIndex) && wCL_DispatchBattleInfo.State == DispatchState.ReadyToComplete)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnAddData(int key, WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			VespaDebug.LogError("!!!!!!!!!! UserDispatchBattleInfoManager::OnAddData - no info");
			SprkDispatchSelection.RefreshOpenLists();
			return;
		}
		if (Get(key) == null)
		{
			base.OnAddData(key, info);
			OnAddDungeonKeyTable(info);
			OnAddDispatchTypeTable(info);
			BroadCast(info, Observer.BroadCastReason.OnAdd);
		}
		else
		{
			UpdateInfo(key, info);
		}
		DispatchInfoListUpdater.Inst.Regist(info);
		SprkDispatchSelection.RefreshOpenLists();
	}

	private void OnAddDungeonKeyTable(WCL_DispatchBattleInfo info)
	{
		if (!_dungeonKeyToInfotable.ContainsKey(info.ChapterIndex))
		{
			_dungeonKeyToInfotable[info.ChapterIndex] = new Dictionary<int, Dictionary<int, WCL_DispatchBattleInfo>>();
		}
		if (!_dungeonKeyToInfotable[info.ChapterIndex].ContainsKey(info.DungeonIndex))
		{
			_dungeonKeyToInfotable[info.ChapterIndex][info.DungeonIndex] = new Dictionary<int, WCL_DispatchBattleInfo>();
		}
		_dungeonKeyToInfotable[info.ChapterIndex][info.DungeonIndex][info.DeckIndex] = info;
	}

	private void OnAddDispatchTypeTable(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(info.ChapterIndex, info.DungeonIndex);
		if (data == null)
		{
			return;
		}
		DispatchType dispatchType_Easy = data.DispatchType_Easy;
		if ((uint)dispatchType_Easy > 1u)
		{
			if (!_dispatchTypeExceptChapterContainListTable.ContainsKey(dispatchType_Easy))
			{
				_dispatchTypeExceptChapterContainListTable[dispatchType_Easy] = new List<WCL_DispatchBattleInfo>();
			}
			if (!_dispatchTypeExceptChapterContainListTable[dispatchType_Easy].Contains(info))
			{
				_dispatchTypeExceptChapterContainListTable[dispatchType_Easy].Add(info);
			}
		}
	}

	private void OnRemove(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			VespaDebug.LogError("!!!!!!!!!! UserDispatchBattleInfoManager::OnRemove - no info");
			return;
		}
		OnRemoveDungeonKeyTable(info);
		OnRemoveDispatchTypeTable(info);
		BroadCast(info, Observer.BroadCastReason.OnRemove);
		DispatchInfoListUpdater.Inst.Remove(info);
	}

	private void OnRemoveDungeonKeyTable(WCL_DispatchBattleInfo info)
	{
		if (_dungeonKeyToInfotable.ContainsKey(info.ChapterIndex) && _dungeonKeyToInfotable[info.ChapterIndex].ContainsKey(info.DungeonIndex) && _dungeonKeyToInfotable[info.ChapterIndex][info.DungeonIndex].ContainsKey(info.DeckIndex))
		{
			_dungeonKeyToInfotable[info.ChapterIndex][info.DungeonIndex].Remove(info.DeckIndex);
			if (_dungeonKeyToInfotable[info.ChapterIndex][info.DungeonIndex].Count == 0)
			{
				_dungeonKeyToInfotable[info.ChapterIndex].Remove(info.DungeonIndex);
			}
			if (_dungeonKeyToInfotable[info.ChapterIndex].Count == 0)
			{
				_dungeonKeyToInfotable.Remove(info.ChapterIndex);
			}
		}
	}

	private void OnRemoveDispatchTypeTable(WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			return;
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(info.ChapterIndex, info.DungeonIndex);
		if (data == null)
		{
			return;
		}
		DispatchType dispatchType_Easy = data.DispatchType_Easy;
		if (!_dispatchTypeExceptChapterContainListTable.ContainsKey(dispatchType_Easy))
		{
			return;
		}
		List<WCL_DispatchBattleInfo> list = _dispatchTypeExceptChapterContainListTable[dispatchType_Easy];
		if (list == null)
		{
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			WCL_DispatchBattleInfo wCL_DispatchBattleInfo = list[i];
			if (wCL_DispatchBattleInfo != null && wCL_DispatchBattleInfo.SlotIndex == info.SlotIndex)
			{
				list.RemoveAt(i);
				break;
			}
		}
		if (_dispatchTypeExceptChapterContainListTable[dispatchType_Easy].Count == 0)
		{
			_dispatchTypeExceptChapterContainListTable.Remove(dispatchType_Easy);
		}
	}

	private void OnUpdateInfo(int key, WCL_DispatchBattleInfo info)
	{
		if (info == null)
		{
			VespaDebug.LogError("!!!!!!!!!! UserDispatchBattleInfoManager::OnUpdateData - no info");
			return;
		}
		WCL_DispatchBattleInfo wCL_DispatchBattleInfo = Get(key);
		base.OnAddData(key, info);
		OnAddDungeonKeyTable(info);
		OnRemoveDispatchTypeTable(info);
		OnAddDispatchTypeTable(info);
		if (wCL_DispatchBattleInfo.State != info.State)
		{
			BroadCast(info, Observer.BroadCastReason.OnChangeState);
		}
		BroadCast(info, Observer.BroadCastReason.OnUpdate);
	}

	private void BroadCast(WCL_DispatchBattleInfo info, Observer.BroadCastReason reason)
	{
		_observer.ObserverManager.AnnounceToObservers(new Observer.BroadCastInfo(info.SlotIndex, info, reason));
	}
}
