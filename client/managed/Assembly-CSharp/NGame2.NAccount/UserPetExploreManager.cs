using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserPetExploreManager : NShared.UserPetExploreManager
{
	public class PetExploreChangeInfo
	{
		public bool IsRemove { get; set; }

		public int ExploreIndex { get; set; }

		public int DeckIndex { get; set; }
	}

	private readonly UserObserverManager<PetExploreChangeInfo> _observerManager;

	private PetExploreInfoKey _cachedKey = new PetExploreInfoKey(0, 0);

	private List<PetExploreInfo> _list = new List<PetExploreInfo>();

	private Dictionary<int, DateTime> _dicExploreEndTimeByPetIndex = new Dictionary<int, DateTime>();

	private DateTime _exploreMinEndTime;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserPetExploreManager()
	{
		_observerManager = new UserObserverManager<PetExploreChangeInfo>();
	}

	public void AddObserver(IUserObserver<PetExploreChangeInfo> userObserver)
	{
		if (userObserver != null)
		{
			_observerManager.AddObserver(userObserver);
		}
	}

	public void RemoveObserver(IUserObserver<PetExploreChangeInfo> userObserver)
	{
		if (userObserver != null)
		{
			_observerManager.RemoveObserver(userObserver);
		}
	}

	public void Apply(PetExploreInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			PetExploreInfo info = infos[i];
			Apply(info);
		}
	}

	public void Apply(PetExploreInfo info)
	{
		if (info != null)
		{
			Add(info);
			SetExploreMinEndTime();
		}
	}

	public override void Set(PetExploreInfo[] values)
	{
		base.Set(values);
		SetExploreMinEndTime();
	}

	protected override void OnAddData(PetExploreInfoKey key, PetExploreInfo value)
	{
		base.OnAddData(key, value);
		AddList(value);
		AddEndTime(value);
	}

	public override bool Remove(PetExploreInfo value)
	{
		if (value == null)
		{
			return false;
		}
		_cachedKey.ExploreIndex = value.ExploreIndex;
		_cachedKey.DeckIndex = value.DeckIndex;
		return Remove(_cachedKey);
	}

	public override bool Remove(PetExploreInfoKey key)
	{
		PetExploreInfo petExploreInfo = Get(key);
		if (petExploreInfo != null)
		{
			List<int> petIndexList = petExploreInfo.GetPetIndexList();
			RemoveEndTime(petIndexList);
		}
		RemoveList(key);
		SetExploreMinEndTime();
		return base.Remove(key);
	}

	public override void RemoveAll()
	{
		base.RemoveAll();
		_list.Clear();
		_dicExploreEndTimeByPetIndex.Clear();
		_exploreMinEndTime = DateTime.MaxValue;
	}

	private void AddList(PetExploreInfo info)
	{
		if (info == null)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < _list.Count; i++)
		{
			PetExploreInfo petExploreInfo = _list[i];
			if (petExploreInfo == null)
			{
				_list.RemoveAt(i);
				i--;
			}
			else if (petExploreInfo.ExploreIndex == info.ExploreIndex && petExploreInfo.DeckIndex == info.DeckIndex)
			{
				_list[i] = info;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_list.Add(info);
		}
	}

	private bool RemoveList(PetExploreInfoKey key)
	{
		if (key == null)
		{
			return false;
		}
		int deckIndex = key.DeckIndex;
		int exploreIndex = key.ExploreIndex;
		for (int i = 0; i < _list.Count; i++)
		{
			PetExploreInfo petExploreInfo = _list[i];
			if (petExploreInfo == null)
			{
				_list.RemoveAt(i);
				i--;
			}
			else if (petExploreInfo.ExploreIndex == exploreIndex && petExploreInfo.DeckIndex == deckIndex)
			{
				_list.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	private void AddEndTime(PetExploreInfo info)
	{
		if (info != null)
		{
			List<int> petIndexList = info.GetPetIndexList();
			Vespa_String_Extension_Method2.TryParseNetworkTime2(info.EndTime, out var o_dateTime);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petIndexList); i++)
			{
				int key = petIndexList[i];
				_dicExploreEndTimeByPetIndex[key] = o_dateTime;
			}
		}
	}

	private void RemoveEndTime(List<int> petIndexList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petIndexList); i++)
		{
			int key = petIndexList[i];
			_dicExploreEndTimeByPetIndex.Remove(key);
		}
	}

	public bool AnyExploreInfo()
	{
		return _list.Count > 0;
	}

	public List<PetExploreInfo> GetPetExploreInfoList()
	{
		return _list;
	}

	public List<PetExploreInfo> GetPetExploreInfoListByExploreIndex(int exploreIndex)
	{
		List<PetExploreInfo> list = null;
		for (int i = 0; i < _list.Count; i++)
		{
			PetExploreInfo petExploreInfo = _list[i];
			if (petExploreInfo != null && petExploreInfo.ExploreIndex == exploreIndex)
			{
				if (list == null)
				{
					list = new List<PetExploreInfo>();
				}
				list.Add(petExploreInfo);
			}
		}
		return list;
	}

	public int GetPetExploreInfoCountByExploreIndex(int exploreIndex)
	{
		int num = 0;
		for (int i = 0; i < _list.Count; i++)
		{
			PetExploreInfo petExploreInfo = _list[i];
			if (petExploreInfo != null && petExploreInfo.ExploreIndex == exploreIndex)
			{
				num++;
			}
		}
		return num;
	}

	public PetExploreInfo Get(int exploreIndex, int deckIndex)
	{
		_cachedKey.ExploreIndex = exploreIndex;
		_cachedKey.DeckIndex = deckIndex;
		return Get(_cachedKey);
	}

	public bool TryGetPetExploreEndTime(int petIndex, out DateTime dateTime)
	{
		return _dicExploreEndTimeByPetIndex.TryGetValue(petIndex, out dateTime);
	}

	public bool AlreadyExploreMemer(int petIndex)
	{
		return _dicExploreEndTimeByPetIndex.ContainsKey(petIndex);
	}

	public DateTime GetCachedExploreMinEndTime()
	{
		return _exploreMinEndTime;
	}

	public void SetExploreMinEndTime()
	{
		_exploreMinEndTime = GetExploreMinEndTime();
	}

	private DateTime GetExploreMinEndTime()
	{
		DateTime dateTime = DateTime.MaxValue;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(_list[i].EndTime, out var o_dateTime))
			{
				o_dateTime = DateTime.MaxValue;
			}
			dateTime = MinTime(dateTime, o_dateTime);
		}
		return dateTime;
	}

	public DateTime GetExploreMinEndTime(int exploreIndex)
	{
		DateTime dateTime = DateTime.MaxValue;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_list); i++)
		{
			PetExploreInfo petExploreInfo = _list[i];
			if (petExploreInfo != null && petExploreInfo.ExploreIndex == exploreIndex)
			{
				if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(petExploreInfo.EndTime, out var o_dateTime))
				{
					o_dateTime = DateTime.MaxValue;
				}
				dateTime = MinTime(dateTime, o_dateTime);
			}
		}
		return dateTime;
	}

	private DateTime MinTime(DateTime lhs, DateTime rhs)
	{
		if (lhs.CompareTo(rhs) <= 0)
		{
			return lhs;
		}
		return rhs;
	}
}
