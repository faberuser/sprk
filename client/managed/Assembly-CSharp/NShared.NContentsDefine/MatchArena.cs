using System;
using System.Collections.Generic;

namespace NShared.NContentsDefine;

[JsonSerializable]
public class MatchArena : ContentsDefine.IDefine
{
	[JsonSerializable]
	public class TimeWithDayOfWeek
	{
		public DayOfWeek DayOfWeek { get; set; }

		public string TimeValue { get; set; }

		public bool IsUtcTime { get; set; }
	}

	[JsonSerializable]
	public class SeasonDesc
	{
		public static SeasonDesc Default = CreateSeasonDescSample();

		public TimeWithDayOfWeek Begin { get; set; }

		public TimeWithDayOfWeek End { get; set; }

		public SeasonDesc()
		{
			Begin = new TimeWithDayOfWeek();
			End = new TimeWithDayOfWeek();
		}
	}

	[JsonSerializable]
	public class ChampionOfMatch
	{
		public MatchPvpType PvpType { get; set; }

		public int TierIndex { get; set; }

		public int MinRank { get; set; }

		public ChampionOfMatch()
		{
		}

		public ChampionOfMatch(MatchPvpType pvpType, int tierIndex, int minRank)
		{
			PvpType = pvpType;
			TierIndex = tierIndex;
			MinRank = minRank;
		}
	}

	private Dictionary<MatchPvpType, List<MatchRewardDataKey2>> _cacheChampions_of_Match;

	public Dictionary<MatchArenaType, SeasonDesc> ArenaSeasonDesc { get; set; }

	public TimeWithDayOfWeek OrdealArena_GiveRewardTime { get; set; }

	public float WaitingTime_for_RequestOfflineMatch { get; set; }

	public float IntervalTime_for_RequestWaitMatch { get; set; }

	public float DelayTime_for_EnterBattleScene { get; set; }

	public float Duration_Short { get; set; }

	public float CurrentSeason_RankerList_Duration_Long { get; set; }

	public float PrevSeason_RankerList_Duration { get; set; }

	public List<ChampionOfMatch> Champions_of_Match { get; set; }

	public MatchArena()
	{
		ArenaSeasonDesc = new Dictionary<MatchArenaType, SeasonDesc>();
		ArenaSeasonDesc.Add(MatchArenaType.Normal, CreateSeasonDescSample());
		ArenaSeasonDesc.Add(MatchArenaType.BanPick, CreateSeasonDescSample());
		OrdealArena_GiveRewardTime = new TimeWithDayOfWeek
		{
			DayOfWeek = DayOfWeek.Monday,
			TimeValue = "00:00"
		};
		WaitingTime_for_RequestOfflineMatch = 10f;
		IntervalTime_for_RequestWaitMatch = 3f;
		DelayTime_for_EnterBattleScene = 2.5f;
		Duration_Short = 30f;
		CurrentSeason_RankerList_Duration_Long = 300f;
		PrevSeason_RankerList_Duration = 3600f;
		Champions_of_Match = new List<ChampionOfMatch>
		{
			new ChampionOfMatch(MatchPvpType.Normal, 70, 1),
			new ChampionOfMatch(MatchPvpType.BanPick, 80, 3),
			new ChampionOfMatch(MatchPvpType.BanPick, 80, 2),
			new ChampionOfMatch(MatchPvpType.BanPick, 80, 1)
		};
	}

	public List<MatchRewardDataKey2> GetChampionsOfMatch(MatchPvpType pvpType)
	{
		List<MatchRewardDataKey2> value;
		lock (Champions_of_Match)
		{
			if (_cacheChampions_of_Match == null)
			{
				_cacheChampions_of_Match = new Dictionary<MatchPvpType, List<MatchRewardDataKey2>>();
				foreach (ChampionOfMatch item in Champions_of_Match)
				{
					if (!_cacheChampions_of_Match.TryGetValue(item.PvpType, out value))
					{
						value = new List<MatchRewardDataKey2>();
						_cacheChampions_of_Match.Add(item.PvpType, value);
					}
					value.Add(new MatchRewardDataKey2(item.TierIndex, item.MinRank));
				}
			}
		}
		if (!_cacheChampions_of_Match.TryGetValue(pvpType, out value))
		{
			return null;
		}
		return value;
	}

	public SeasonDesc GetSeasonDesc(MatchArenaType arenaType)
	{
		if (ArenaSeasonDesc.TryGetValue(arenaType, out var value))
		{
			return value;
		}
		return SeasonDesc.Default;
	}

	private static SeasonDesc CreateSeasonDescSample()
	{
		return new SeasonDesc
		{
			Begin = 
			{
				DayOfWeek = DayOfWeek.Monday,
				TimeValue = "01:00"
			},
			End = 
			{
				DayOfWeek = DayOfWeek.Saturday,
				TimeValue = "23:59"
			}
		};
	}
}
