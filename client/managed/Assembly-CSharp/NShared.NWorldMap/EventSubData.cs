using System;

namespace NShared.NWorldMap;

public class EventSubData : ITimerEvent
{
	public int EventIndex;

	public EventType EventType;

	public int ChapterIndex;

	public int DungeonIndex;

	public int ProgressCount;

	public int RequireCount;

	public string RewardItems;

	public int RewardGold;

	public bool Completed;

	public bool NoExpireOnComplete;

	public DateTime OccuredTime;

	public int Duration;

	public int GetRemainTime()
	{
		if (Duration == 0)
		{
			return int.MaxValue;
		}
		if (Completed && NoExpireOnComplete)
		{
			return int.MaxValue;
		}
		return Math.Max((int)(OccuredTime.AddSeconds(Duration) - DateTime.UtcNow).TotalSeconds, 0);
	}

	bool ITimerEvent.IsExpired()
	{
		return GetRemainTime() <= 0;
	}
}
