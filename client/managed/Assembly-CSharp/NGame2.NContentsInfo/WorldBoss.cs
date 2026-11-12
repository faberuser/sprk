using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetWorldBossInfo;
using NShared.GetWorldBossRankInfo;
using NShared.GetWorldBossRankerList;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NContentsInfo;

public class WorldBoss : SingletonUpdater<WorldBoss>
{
	private struct RankerListCache
	{
		public WorldBossRankerInfo[] RankerList;

		public float CachedTime;
	}

	private class SeasonData<T>
	{
		public T Info;

		public WorldBossRankerInfo RankInfo;

		public int TotalRankerCount;

		public Dictionary<Tuple<int, int>, RankerListCache> RankerCache;
	}

	private struct PrevSeason
	{
		public int WorldBossIndex;

		public int Season;

		public bool IsEmpty()
		{
			if (WorldBossIndex != 0)
			{
				return Season == 0;
			}
			return true;
		}
	}

	private struct Result_for_GetWorldBossRankInfo
	{
		public WorldBossRankerInfo Info;

		public int TotalRankerCount;
	}

	public const int RankerPerPage = 10;

	public const float TimeForCurrentSeasonCacheInvalidate = 10f;

	public const float TimeForPrevSeasonCacheInvalidate = 86400f;

	private SeasonData<WorldBossInfo> _currentSeason = new SeasonData<WorldBossInfo>();

	private SeasonData<PrevSeason> _prevSeason = new SeasonData<PrevSeason>();

	public WorldBossInfo WorldBossInfo => _currentSeason.Info;

	public WorldBossData WorldBossData
	{
		get
		{
			if (_currentSeason.Info == null)
			{
				return null;
			}
			return GameRoot.Instance.WorldBossDataContainer.GetData(_currentSeason.Info.Index);
		}
	}

	public WorldBossRankerInfo RankInfo => _currentSeason.RankInfo;

	public int TotalRankerCount => _currentSeason.TotalRankerCount;

	public int LastWorldBossRank { get; private set; }

	public long LastWorldBossScore { get; private set; }

	public WorldBossData GetCurrentWorldBossData()
	{
		if (_currentSeason.Info == null)
		{
			return null;
		}
		return GameRoot.Instance.WorldBossDataContainer.GetData(_currentSeason.Info.Index);
	}

	public void ClearLastWorldBossInfo()
	{
		LastWorldBossRank = -1;
		LastWorldBossScore = 0L;
	}

	public void ApplyWorldBossRankerInfo(WorldBossRankerInfo rankInfo)
	{
		if (rankInfo != null && rankInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId)
		{
			WorldBossRankerInfo rankInfo2 = _currentSeason.RankInfo;
			_currentSeason.RankInfo = rankInfo;
			LastWorldBossRank = rankInfo2?.Rank ?? rankInfo.Rank;
			LastWorldBossScore = rankInfo2?.Score ?? rankInfo.Score;
		}
	}

	public void RequestWorldBossInfo(bool showLoadingGauge, Action<WorldBossInfo> onRequest)
	{
		if (!AccountManager.IsLogin())
		{
			Vespa_System_Action_Extension_Method.fire(onRequest, null);
			return;
		}
		NShared.GetWorldBossInfo.Request request = new NShared.GetWorldBossInfo.Request();
		WebRequestFlag flag = (showLoadingGauge ? WebRequestFlag.ShowLoadingGauge : WebRequestFlag.Ping);
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.GetWorldBossInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onRequest, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onRequest, null);
			}
			else if (response.Result != NShared.GetWorldBossInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onRequest, null);
			}
			else if (__Extension_Method_ICollection__.COUNT(response.WorldBossInfos) == 0)
			{
				Vespa_System_Action_Extension_Method.fire(onRequest, null);
			}
			else
			{
				SetWorldBossInfo(response.WorldBossInfos[0]);
				Vespa_System_Action_Extension_Method.fire(onRequest, _currentSeason.Info);
			}
		});
	}

	public void SetWorldBossInfo(WorldBossInfo info)
	{
		_currentSeason.Info = info;
		if (_currentSeason.Info != null)
		{
			if (!GetPrevWorldBossSeason(WorldBossInfo, out var prevWorldBossIndex, out var prevSeasonIndex))
			{
				_prevSeason = new SeasonData<PrevSeason>();
			}
			else if (_prevSeason.Info.WorldBossIndex != prevWorldBossIndex || _prevSeason.Info.Season != prevSeasonIndex)
			{
				_prevSeason = new SeasonData<PrevSeason>();
				_prevSeason.Info.WorldBossIndex = prevWorldBossIndex;
				_prevSeason.Info.Season = prevSeasonIndex;
			}
		}
	}

	public void RequestCurrentRankInfo(Action<WorldBossRankerInfo> onComplete, Action<int> onNext = null)
	{
		if (WorldBossInfo == null)
		{
			RequestWorldBossInfo(showLoadingGauge: false, delegate(WorldBossInfo info)
			{
				if (info == null)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete, null);
					Vespa_System_Action_Extension_Method.fire(onNext, 0);
				}
				else
				{
					RequestCurrentRankInfoInternal(onComplete, onNext);
				}
			});
		}
		else
		{
			RequestCurrentRankInfoInternal(onComplete, onNext);
		}
	}

	private void RequestCurrentRankInfoInternal(Action<WorldBossRankerInfo> onComplete, Action<int> onNext)
	{
		int index = WorldBossInfo.Index;
		int season = WorldBossInfo.Season;
		RequestWorldBossRankInfo(index, season, delegate(Result_for_GetWorldBossRankInfo result)
		{
			ApplyWorldBossRankerInfo(result.Info);
			_currentSeason.TotalRankerCount = result.TotalRankerCount;
			Vespa_System_Action_Extension_Method.fire(onComplete, result.Info);
			Vespa_System_Action_Extension_Method.fire(onNext, 0);
		});
	}

	public void RequestPrevRankInfo(Action<WorldBossRankerInfo> onComplete, Action<int> onNext = null)
	{
		if (_prevSeason.Info.IsEmpty())
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, null);
			return;
		}
		if (_prevSeason.RankInfo != null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, _prevSeason.RankInfo);
			Vespa_System_Action_Extension_Method.fire(onNext, 0);
			return;
		}
		int worldBossIndex = _prevSeason.Info.WorldBossIndex;
		int season = _prevSeason.Info.Season;
		RequestWorldBossRankInfo(worldBossIndex, season, delegate(Result_for_GetWorldBossRankInfo result)
		{
			_prevSeason.RankInfo = result.Info;
			_prevSeason.TotalRankerCount = result.TotalRankerCount;
			Vespa_System_Action_Extension_Method.fire(onComplete, result.Info);
			Vespa_System_Action_Extension_Method.fire(onNext, 0);
		});
	}

	public void GetCurrentRankerList(int page, Action<WorldBossRankerInfo[]> onList)
	{
		if (WorldBossInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		int index = WorldBossInfo.Index;
		int season = WorldBossInfo.Season;
		RequestRankerList(_currentSeason, 10f, index, season, page, onList);
	}

	public void GetPrevRankerList(int page, Action<WorldBossRankerInfo[]> onList)
	{
		if (_prevSeason.Info.IsEmpty())
		{
			Vespa_System_Action_Extension_Method.fire(onList, null);
			return;
		}
		int worldBossIndex = _prevSeason.Info.WorldBossIndex;
		int season = _prevSeason.Info.Season;
		RequestRankerList(_prevSeason, 86400f, worldBossIndex, season, page, onList);
	}

	private static void RequestWorldBossRankInfo(int worldBossIndex, int seasonIndex, Action<Result_for_GetWorldBossRankInfo> onComplete)
	{
		NShared.GetWorldBossRankInfo.Request request = new NShared.GetWorldBossRankInfo.Request();
		request.WorldBossIndex = worldBossIndex;
		request.Season = seasonIndex;
		request.RankingType = WorldBossRankingType.World;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetWorldBossRankInfo.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onComplete, default(Result_for_GetWorldBossRankInfo));
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onComplete, default(Result_for_GetWorldBossRankInfo));
			}
			else if (response.Result != NShared.GetWorldBossRankInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete, default(Result_for_GetWorldBossRankInfo));
			}
			else
			{
				Result_for_GetWorldBossRankInfo i_argA = new Result_for_GetWorldBossRankInfo
				{
					Info = response.RankInfo,
					TotalRankerCount = response.TotalRankerCount
				};
				Vespa_System_Action_Extension_Method.fire(onComplete, i_argA);
			}
		});
	}

	private static void RequestRankerList<T>(SeasonData<T> seasonData, float timeForCacheInvalidate, int worldBossIndex, int seasonIndex, int page, Action<WorldBossRankerInfo[]> onList)
	{
		Tuple<int, int> cachekey = Tuple.Create(seasonIndex, page);
		if (seasonData.RankerCache != null && seasonData.RankerCache.TryGetValue(cachekey, out var value) && Time.time - value.CachedTime < timeForCacheInvalidate)
		{
			Vespa_System_Action_Extension_Method.fire(onList, value.RankerList);
			return;
		}
		NShared.GetWorldBossRankerList.Request request = new NShared.GetWorldBossRankerList.Request();
		request.WorldBossIndex = worldBossIndex;
		request.Season = seasonIndex;
		request.PageNo = page;
		request.RankingType = WorldBossRankingType.World;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetWorldBossRankerList.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else if (response.Result != NShared.GetWorldBossRankerList.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onList, null);
			}
			else
			{
				RankerListCache value2 = new RankerListCache
				{
					RankerList = response.RankerInfos,
					CachedTime = Time.time
				};
				seasonData.RankerCache = seasonData.RankerCache ?? new Dictionary<Tuple<int, int>, RankerListCache>();
				seasonData.RankerCache[cachekey] = value2;
				Vespa_System_Action_Extension_Method.fire(onList, value2.RankerList);
			}
		});
	}

	public override void ChainUpdate()
	{
	}

	private static bool GetPrevWorldBossSeason(WorldBossInfo info, out int prevWorldBossIndex, out int prevSeasonIndex)
	{
		prevWorldBossIndex = 0;
		prevSeasonIndex = 0;
		if (info == null)
		{
			return false;
		}
		int index = info.Index;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(index);
		if (data == null)
		{
			VespaDebug.LogError("GetPrevWorldBossSeason failed: currentData is null. currentWorldBossIndex=" + index);
			return false;
		}
		foreach (WorldBossData allDatum in GameRoot.Instance.WorldBossDataContainer.GetAllData())
		{
			if (allDatum.NextIndex == index && allDatum.IsGlobal == data.IsGlobal)
			{
				prevWorldBossIndex = allDatum.Index;
				break;
			}
		}
		if (prevWorldBossIndex == 0)
		{
			VespaDebug.LogError("GetPrevWorldBossSeason failed: prevWorldBossIndex is null. currentWorldBossIndex=" + index);
			return false;
		}
		prevSeasonIndex = info.Season - 1;
		return true;
	}
}
