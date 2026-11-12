using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI;
using NShared;
using NShared.GetMatchRank;
using NShared.NContentsDefine;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NContentsDefine;

public class MatchArena
{
	private static Dictionary<DayOfWeek, DateTime> _sampleForDayOfWeek;

	static MatchArena()
	{
		_sampleForDayOfWeek = new Dictionary<DayOfWeek, DateTime>();
		DateTime value = new DateTime(2022, 1, 2, 1, 0, 0);
		for (int i = 0; i < 20; i++)
		{
			if (_sampleForDayOfWeek.ContainsKey(value.DayOfWeek))
			{
				break;
			}
			_sampleForDayOfWeek.Add(value.DayOfWeek, value);
			value = value.AddDays(1.0);
		}
		if (_sampleForDayOfWeek.Count != 7)
		{
			VespaDebug.LogError("[[[[[ Invalid Sampling DayOfWeek ]]]]]");
		}
	}

	public static string GetString_of_TimeWithDayOfWeek(NShared.NContentsDefine.MatchArena.TimeWithDayOfWeek timeData)
	{
		if (!_sampleForDayOfWeek.TryGetValue(timeData.DayOfWeek, out var value))
		{
			return "(invalid sample)";
		}
		if (!DateTime.TryParse(timeData.TimeValue, out var result))
		{
			return "(invalid time)";
		}
		DateTime time = new DateTime(value.Year, value.Month, value.Day, result.Hour, result.Minute, result.Second);
		if (timeData.IsUtcTime)
		{
			time = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(time);
		}
		return LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(time.DayOfWeek)) + time.ToString(" HH:mm");
	}

	public static string GetLocalTimeString(DateTime localTime)
	{
		return LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(localTime.DayOfWeek)) + localTime.ToString("HH:mm");
	}

	public static bool TryGetArenaType(RecordsOfHonorContentType contentType, out MatchArenaType arenaType)
	{
		switch (contentType)
		{
		case RecordsOfHonorContentType.Match:
			arenaType = MatchArenaType.Normal;
			return true;
		case RecordsOfHonorContentType.BanPick:
			arenaType = MatchArenaType.BanPick;
			return true;
		default:
			arenaType = MatchArenaType.Normal;
			return false;
		}
	}

	public static IList<MatchTierType> GetTierList(MatchArenaType arenaType)
	{
		if (arenaType == MatchArenaType.BanPick)
		{
			return GameRoot.Instance.GlobalBanPickTierDataContainer.GetAllTierTypeList();
		}
		return GameRoot.Instance.MatchTierDataContainer.GetAllTierTypeList();
	}

	public static int GetTierIndex(MatchArenaType arenaType, ArenaBattleInfo info)
	{
		return info?.WorldTierIndex ?? 0;
	}

	public static int GetChampionRank(MatchArenaType arenaType)
	{
		if (arenaType == MatchArenaType.Normal || arenaType == MatchArenaType.BanPick)
		{
			return 3;
		}
		return 1;
	}

	public static bool IsGlobal(MatchArenaType arenaType)
	{
		if (arenaType == MatchArenaType.Normal || arenaType == MatchArenaType.BanPick || arenaType == MatchArenaType.Ordeal)
		{
			return true;
		}
		return false;
	}

	public static MatchTierData GetTierData(MatchArenaType arenaType, int tierIndex)
	{
		if (arenaType == MatchArenaType.BanPick)
		{
			return GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(tierIndex);
		}
		return GameRoot.Instance.MatchTierDataContainer.GetData(tierIndex);
	}

	public static ArenaBattleInfo GetArenaBattleInfo(MatchArenaType arenaType, NShared.GetMatchRank.Response response)
	{
		return arenaType switch
		{
			MatchArenaType.WorldBanPick => response.BattleBanPickInfo, 
			MatchArenaType.BanPick => response.BattleBanPickInfo, 
			MatchArenaType.Normal => response.BattleInfo, 
			_ => null, 
		};
	}

	public static DateTime GetCurrentServerTime(MatchArenaType arenaType)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
	}

	public static string GetBattleDefineCode(MatchArenaType arenaType)
	{
		return arenaType switch
		{
			MatchArenaType.Normal => "Arena", 
			MatchArenaType.BanPick => "Arena2", 
			_ => string.Empty, 
		};
	}
}
