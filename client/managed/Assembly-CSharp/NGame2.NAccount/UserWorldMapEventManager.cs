using System;
using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NWorldMap;
using NVespa;

namespace NGame2.NAccount;

public class UserWorldMapEventManager : NShared.UserWorldMapEventManager
{
	public List<WorldMapEventInfo> Get(int chapterIndex, int dungeonIndex)
	{
		List<WorldMapEventInfo> list = new List<WorldMapEventInfo>();
		using IEnumerator<WorldMapEventInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			WorldMapEventInfo current = enumerator.Current;
			if (current.ChapterIndex == chapterIndex && current.DungeonIndex == dungeonIndex && !IsExpired(current.EventIndex, chapterIndex, dungeonIndex))
			{
				list.Add(current);
			}
		}
		return list;
	}

	public WorldMapEventInfo Get(EventType eventType, int chapterIndex, int dungeonIndex)
	{
		using (IEnumerator<WorldMapEventInfo> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				WorldMapEventInfo current = enumerator.Current;
				if (current.ChapterIndex == chapterIndex && current.DungeonIndex == dungeonIndex && current.EventType == eventType && !IsExpired(current.EventIndex, chapterIndex, dungeonIndex))
				{
					return current;
				}
			}
		}
		return null;
	}

	public T GetEventData<T>(int chapterIndex, int dungeonIndex) where T : EventSubData
	{
		using PoolingList<T> poolingList = GetEventDataOfType<T>();
		for (int i = 0; i < poolingList.Count; i++)
		{
			T val = poolingList[i];
			if (val.ChapterIndex == chapterIndex && val.DungeonIndex == dungeonIndex)
			{
				return val;
			}
		}
		return null;
	}

	public T ToEventData<T>(WorldMapEventInfo info) where T : EventSubData
	{
		if (info == null)
		{
			return null;
		}
		if (IsExpired(info))
		{
			return null;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.OccuredTime, out var o_dateTime))
		{
			return null;
		}
		T val = JsonMarshaler.Parse<T>(info.SerializedSubData);
		val.EventIndex = info.EventIndex;
		val.EventType = info.EventType;
		val.ChapterIndex = info.ChapterIndex;
		val.DungeonIndex = info.DungeonIndex;
		val.ProgressCount = info.ProgressCount;
		val.RequireCount = info.RequireCount;
		val.RewardGold = info.RewardGold;
		val.RewardItems = info.RewardItems;
		val.Completed = info.Completed;
		val.NoExpireOnComplete = info.NoExpireOnComplete;
		val.OccuredTime = o_dateTime;
		val.Duration = info.Duration;
		return val;
	}

	public PoolingList<T> GetEventDataOfType<T>() where T : EventSubData
	{
		PoolingList<T> result = default(PoolingList<T>);
		string name = typeof(T).Name;
		EventType o_value = EventType.None;
		EnumLookup<EventType>.TryParse(name, out o_value);
		if (o_value == EventType.None)
		{
			return result;
		}
		using IEnumerator<WorldMapEventInfo> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			WorldMapEventInfo current = enumerator.Current;
			if (current.EventType == o_value)
			{
				T val = ToEventData<T>(current);
				if (val != null)
				{
					result.Add(val);
				}
			}
		}
		return result;
	}

	public int GetRemainTime(WorldMapEventInfo info)
	{
		if (info == null)
		{
			return 0;
		}
		if (info.Duration <= 0)
		{
			return int.MaxValue;
		}
		if (info.Completed && info.NoExpireOnComplete)
		{
			return int.MaxValue;
		}
		DateTime o_dateTime = Common.GetUTCNow();
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.OccuredTime, out o_dateTime))
		{
			return 0;
		}
		int num = (int)(Common.GetUTCNow() - o_dateTime).TotalSeconds;
		return System.Math.Max(info.Duration - num, 0);
	}

	public int GetRemainTime(int eventIndex, int chapterIndex, int dungeonIndex)
	{
		WorldMapEventInfo info = Get(eventIndex, chapterIndex, dungeonIndex);
		return GetRemainTime(info);
	}

	public bool IsExpired(WorldMapEventInfo info)
	{
		return GetRemainTime(info) <= 0;
	}

	public bool IsExpired(int eventIndex, int chapterIndex, int dungeonIndex)
	{
		return GetRemainTime(eventIndex, chapterIndex, dungeonIndex) <= 0;
	}
}
