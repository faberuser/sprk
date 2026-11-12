using System;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetMatchRank;
using NShared.GetSeasonInfo;
using NShared.GetWorldRanker;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NContentsInfo;

public class MatchArena : SingletonAuto<MatchArena>
{
	private struct InfoCache<T> where T : class
	{
		public T Value { get; private set; }

		public float Timestamp { get; private set; }

		public float ElapsedTime => Time.time - Timestamp;

		public InfoCache(T value)
		{
			Value = value;
			Timestamp = ((Value != null) ? Time.time : (-999999f));
		}
	}

	private class DataForPvpType
	{
		public InfoCache<ArenaBattleInfo> BattleInfo = new InfoCache<ArenaBattleInfo>(null);

		public InfoCache<RankResultInfo> RankResultInfo = new InfoCache<RankResultInfo>(null);

		public MatchSeasonInfo SeasonInfo;
	}

	public struct RankClientInfo
	{
		public RankResultInfo Original { get; private set; }

		public RankClientInfo(RankResultInfo info)
		{
			Original = info;
		}
	}

	public class MatchSeasonInfo
	{
		private struct RankerListKey
		{
			public MatchTierType TierType;

			public int Page;
		}

		private class RankerListCache
		{
			public MatchRankerInfo[] Value;

			public float Timestamp;

			public RankerListCache(MatchRankerInfo[] value)
			{
				Value = value;
				Timestamp = Time.time;
			}
		}

		private MatchSeasonData _seasonData;

		private Dictionary<RankerListKey, RankerListCache> _currentRankerListCache = new Dictionary<RankerListKey, RankerListCache>();

		private Dictionary<RankerListKey, RankerListCache> _prevRankerListCache = new Dictionary<RankerListKey, RankerListCache>();

		public int SeasonIndex
		{
			get
			{
				if (ArenaType == MatchArenaType.Normal)
				{
					return _seasonData.Index;
				}
				return _seasonData.SeasonIndex;
			}
		}

		public int PrevRegularSeasonIndex
		{
			get
			{
				if (ArenaType == MatchArenaType.BanPick && _seasonData.MatchSeasonType == MatchSeasonType.Regular)
				{
					return SeasonIndex - 2;
				}
				return SeasonIndex - 1;
			}
		}

		public MatchPvpType PvpType { get; private set; }

		public MatchArenaType ArenaType => _seasonData.ArenaType;

		public DateTime BeginServerLocal { get; private set; }

		public DateTime EndServerLocal { get; private set; }

		public DateTime BeginUtc { get; private set; }

		public DateTime EndUtc { get; private set; }

		public MatchSeasonType MatchSeasonType => _seasonData.MatchSeasonType;

		public bool ActiveTier => _seasonData.ActiveTier;

		public int LimitedHeroCount => _seasonData.LimitedHeroCount;

		public MatchSeasonInfo(MatchPvpType pvpType, MatchSeasonData seasonData)
		{
			PvpType = pvpType;
			_seasonData = seasonData;
			BeginUtc = DateTime.SpecifyKind(Convert.ToDateTime(_seasonData.Begin), DateTimeKind.Utc);
			EndUtc = DateTime.SpecifyKind(Convert.ToDateTime(_seasonData.End), DateTimeKind.Utc);
			BeginServerLocal = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(BeginUtc);
			EndServerLocal = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(EndUtc);
		}

		public void ClearOldRankerList(float timeLength)
		{
			float time = Time.time;
			foreach (KeyValuePair<RankerListKey, RankerListCache> item in _currentRankerListCache)
			{
				RankerListCache value = item.Value;
				if (!(time - value.Timestamp < timeLength))
				{
					value.Timestamp = -9999999f;
				}
			}
		}

		public void RequestCurrentRankerList(MatchTierType tierType, int page, Action<MatchRankerInfo[]> onList)
		{
			RankerListKey key = new RankerListKey
			{
				TierType = tierType,
				Page = page
			};
			if (_currentRankerListCache.TryGetValue(key, out var value) && Time.time - value.Timestamp < ContentsDefine.MatchArena.CurrentSeason_RankerList_Duration_Long)
			{
				Vespa_System_Action_Extension_Method.fire(onList, value.Value);
				return;
			}
			RequestRankerList(0, key, delegate(MatchRankerInfo[] listFromRequest)
			{
				_currentRankerListCache[key] = new RankerListCache(listFromRequest);
				Vespa_System_Action_Extension_Method.fire(onList, listFromRequest);
			});
		}

		public void RequestPrevRankerList(MatchTierType tierType, int page, Action<MatchRankerInfo[]> onList)
		{
			RankerListKey key = new RankerListKey
			{
				TierType = tierType,
				Page = page
			};
			if (_prevRankerListCache.TryGetValue(key, out var value) && Time.time - value.Timestamp < ContentsDefine.MatchArena.PrevSeason_RankerList_Duration)
			{
				Vespa_System_Action_Extension_Method.fire(onList, value.Value);
				return;
			}
			RequestRankerList(PrevRegularSeasonIndex, key, delegate(MatchRankerInfo[] listFromRequest)
			{
				_prevRankerListCache[key] = new RankerListCache(listFromRequest);
				Vespa_System_Action_Extension_Method.fire(onList, listFromRequest);
			});
		}

		private void RequestRankerList(int seasonIndex, RankerListKey key, Action<MatchRankerInfo[]> onList)
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("RankerPerPage");
			NShared.GetWorldRanker.Request request = new NShared.GetWorldRanker.Request();
			request.ArenaType = ArenaType;
			request.TierType = key.TierType;
			request.StartRank = key.Page * valueInt;
			request.EndRank = (key.Page + 1) * valueInt - 1;
			request.SeasonIndex = seasonIndex;
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetWorldRanker.Response res)
			{
				if (res == null || res.BaseResult != BaseResultType.Success)
				{
					Vespa_System_Action_Extension_Method.fire(onList, null);
				}
				else if (res.Result != NShared.GetWorldRanker.ResultType.Success)
				{
					MessagePopup.OpenError(res.Result);
					Vespa_System_Action_Extension_Method.fire(onList, null);
				}
				else
				{
					Vespa_System_Action_Extension_Method.fire(onList, res.Rankers);
				}
			});
		}
	}

	private Dictionary<MatchPvpType, DataForPvpType> _dataForPvp = new Dictionary<MatchPvpType, DataForPvpType>();

	public string Dump()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<MatchPvpType, DataForPvpType> item in _dataForPvp)
		{
			stringBuilder.AppendLine($"[{item.Key}]");
			if (item.Value.SeasonInfo == null)
			{
				stringBuilder.AppendLine(" - SeasonInfo is null");
				continue;
			}
			string value = $" - SeasonInfo: Begin=(Utc={Vespa_DateTime_Extension_Method.ToNetworkString2(item.Value.SeasonInfo.BeginUtc)}, Local={Vespa_DateTime_Extension_Method.ToNetworkString2(item.Value.SeasonInfo.BeginServerLocal)})";
			string value2 = $" - SeasonInfo: End=(Utc={Vespa_DateTime_Extension_Method.ToNetworkString2(item.Value.SeasonInfo.EndUtc)}, Local={Vespa_DateTime_Extension_Method.ToNetworkString2(item.Value.SeasonInfo.EndServerLocal)})";
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(value2);
		}
		return stringBuilder.ToString();
	}

	public ArenaBattleInfo GetArenaBattleInfo(MatchArenaType arenaType)
	{
		if (!arenaType.TryToPvpType(out var pvpType))
		{
			return null;
		}
		return GetDataForPvpType(pvpType).BattleInfo.Value;
	}

	public MatchSeasonInfo GetOrdealSeasonInfo()
	{
		return GetMatchSeasonInfo(MatchPvpType.Normal);
	}

	public MatchSeasonInfo GetMatchSeasonInfo(MatchArenaType arenaType)
	{
		if (!arenaType.TryToPvpType(out var pvpType))
		{
			return null;
		}
		return GetDataForPvpType(pvpType).SeasonInfo;
	}

	public ArenaBattleInfo GetArenaBattleInfo(MatchPvpType pvpType)
	{
		return GetDataForPvpType(pvpType).BattleInfo.Value;
	}

	public RankResultInfo GetRankResultInfo(MatchPvpType pvpType)
	{
		return GetDataForPvpType(pvpType).RankResultInfo.Value;
	}

	public MatchSeasonInfo GetMatchSeasonInfo(MatchPvpType pvpType)
	{
		return GetDataForPvpType(pvpType).SeasonInfo;
	}

	private DataForPvpType GetDataForPvpType(MatchPvpType pvpType)
	{
		if (!_dataForPvp.TryGetValue(pvpType, out var value))
		{
			value = new DataForPvpType();
			_dataForPvp.Add(pvpType, value);
		}
		return value;
	}

	private void RemoveSeasonInfo(MatchPvpType pvpType)
	{
		GetDataForPvpType(pvpType).SeasonInfo = null;
	}

	public void ApplyBattleInfo(PlayerBattleInfo battleInfo, PlayerBattleBanPickInfo battleBanPickInfo)
	{
		ApplyBattleInfo(MatchPvpType.Normal, battleInfo);
		ApplyBattleInfo(MatchPvpType.BanPick, battleBanPickInfo);
	}

	public void ApplyBattleInfo(MatchPvpType pvpType, ArenaBattleInfo battleInfo)
	{
		if (battleInfo != null)
		{
			GetDataForPvpType(pvpType).BattleInfo = new InfoCache<ArenaBattleInfo>(battleInfo);
		}
	}

	public void ApplySeasonData(MatchPvpType pvpType, MatchSeasonData seasonData)
	{
		if (seasonData != null)
		{
			GetDataForPvpType(pvpType).SeasonInfo = CreateSeasonInfo(pvpType, seasonData);
		}
	}

	public void ApplyRankResult(MatchPvpType pvpType, RankResultInfo rankResultInfo)
	{
		if (rankResultInfo != null)
		{
			GetDataForPvpType(pvpType).RankResultInfo = new InfoCache<RankResultInfo>(rankResultInfo);
		}
	}

	public void RequestMatchRank(MatchPvpType pvpType, Action<MatchTierType, int> onComplete)
	{
		RequestMatchRankInternal(pvpType, delegate(ArenaBattleInfo battleInfo)
		{
			if (battleInfo == null)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, MatchTierType.Bronze, 0);
			}
			else
			{
				MatchTierType i_argA = MatchTierType.Bronze;
				if (battleInfo != null)
				{
					int tierIndex = NGame2.NContentsDefine.MatchArena.GetTierIndex(pvpType.ToMatchArenaType(), battleInfo);
					MatchTierData tierData = NGame2.NContentsDefine.MatchArena.GetTierData(pvpType.ToMatchArenaType(), tierIndex);
					if (tierData != null)
					{
						i_argA = tierData.Tier;
					}
				}
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA, 0);
			}
		});
	}

	private void RequestMatchRankInternal(MatchPvpType pvpType, Action<ArenaBattleInfo> onComplete)
	{
		DataForPvpType dataForPvpType = GetDataForPvpType(pvpType);
		InfoCache<ArenaBattleInfo> battleInfo = dataForPvpType.BattleInfo;
		InfoCache<RankResultInfo> rankResultInfo = dataForPvpType.RankResultInfo;
		if (Mathf.Max(battleInfo.ElapsedTime, rankResultInfo.ElapsedTime) < ContentsDefine.MatchArena.Duration_Short)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, battleInfo.Value);
			return;
		}
		NShared.GetMatchRank.Request request = new NShared.GetMatchRank.Request();
		request.ArenaType = pvpType.ToMatchArenaType();
		request.RankerAccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetMatchRank.Response res)
		{
			if (res == null || res.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else if (res.Result != NShared.GetMatchRank.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else
			{
				ApplyRankResult(pvpType, res.RankResult);
				ApplyBattleInfo(res.BattleInfo, res.BattleBanPickInfo);
				ArenaBattleInfo arenaBattleInfo = GetArenaBattleInfo(pvpType);
				Vespa_System_Action_Extension_Method.fire(onComplete, arenaBattleInfo);
			}
		});
	}

	public void RequestOrdealSeasonInfo(Action<MatchSeasonInfo> onComplete)
	{
		RequestSeasonInfo(MatchPvpType.Normal, onComplete);
	}

	public void RequestSeasonInfo(MatchPvpType pvpType, Action<MatchSeasonInfo> onComplete)
	{
		MatchSeasonInfo matchSeasonInfo = GetMatchSeasonInfo(pvpType);
		if (matchSeasonInfo != null)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() <= matchSeasonInfo.EndUtc)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, matchSeasonInfo);
				return;
			}
			RemoveSeasonInfo(pvpType);
		}
		NShared.GetSeasonInfo.Request request = new NShared.GetSeasonInfo.Request
		{
			ArenaType = pvpType.ToMatchArenaType()
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetSeasonInfo.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != NShared.GetSeasonInfo.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else
			{
				ApplySeasonData(pvpType, response.SeasonData);
				Vespa_System_Action_Extension_Method.fire(onComplete, GetMatchSeasonInfo(pvpType));
			}
		});
	}

	public void ClearOldRankerList(MatchPvpType pvpType, float timeLength)
	{
		MatchSeasonInfo matchSeasonInfo = GetMatchSeasonInfo(pvpType);
		if (matchSeasonInfo != null && !(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() > matchSeasonInfo.EndUtc))
		{
			matchSeasonInfo.ClearOldRankerList(timeLength);
		}
	}

	public void RequestCurrentRankerList(MatchPvpType pvpType, MatchTierType tierType, int page, Action<MatchRankerInfo[]> onList)
	{
		RequestSeasonInfo(pvpType, delegate(MatchSeasonInfo seasonData)
		{
			if (seasonData == null)
			{
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				seasonData.RequestCurrentRankerList(tierType, page, onList);
			}
		});
	}

	public void RequestPrevRankerList(MatchPvpType pvpType, MatchTierType tierType, int page, Action<MatchRankerInfo[]> onList)
	{
		RequestSeasonInfo(pvpType, delegate(MatchSeasonInfo seasonData)
		{
			if (seasonData == null)
			{
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				seasonData.RequestPrevRankerList(tierType, page, onList);
			}
		});
	}

	private static MatchSeasonInfo CreateSeasonInfo(MatchPvpType pvpType, MatchSeasonData seasonData)
	{
		if (seasonData == null)
		{
			return null;
		}
		return new MatchSeasonInfo(pvpType, seasonData);
	}
}
