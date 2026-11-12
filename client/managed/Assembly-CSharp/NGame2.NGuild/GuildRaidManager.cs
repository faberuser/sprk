using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NGuild;

public class GuildRaidManager
{
	private readonly Dictionary<long, GuildRaidMemberTotalScoreInfo> _scoreDictionary = new Dictionary<long, GuildRaidMemberTotalScoreInfo>();

	private readonly Dictionary<int, GuildRaidDungeonInfo> _guildRaidDungeonInfoDic = new Dictionary<int, GuildRaidDungeonInfo>();

	private LocalAccountString _openTimeLocalAccount = new LocalAccountString(string.Empty, "Guild_Raid_Open_Time");

	public Dictionary<int, GuildRaidDungeonInfo> GuildRaidDungeonInfoDic => _guildRaidDungeonInfoDic;

	public GuildRaidDungeonInfo NormalGuildRaidDungeonInfo
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_guildRaidDungeonInfoDic))
			{
				return null;
			}
			List<GuildRaidDungeonInfo> list = _guildRaidDungeonInfoDic.Values.ToList();
			if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				return null;
			}
			GuildRaidDungeonInfo guildRaidDungeonInfo = null;
			for (int i = 0; i < list.Count(); i++)
			{
				guildRaidDungeonInfo = list[i];
				if (guildRaidDungeonInfo != null && !guildRaidDungeonInfo.CheckBonusDungeon && !guildRaidDungeonInfo.CheckDummyDungeon)
				{
					return guildRaidDungeonInfo;
				}
			}
			return null;
		}
	}

	public bool CheckOpenTime
	{
		get
		{
			GuildRaidDungeonInfo normalGuildRaidDungeonInfo = NormalGuildRaidDungeonInfo;
			if (normalGuildRaidDungeonInfo == null)
			{
				return false;
			}
			if (normalGuildRaidDungeonInfo.OpenedTime.Equals(_openTimeLocalAccount.Value))
			{
				return false;
			}
			return true;
		}
	}

	public void Apply(GuildRaidDungeonInfo info)
	{
		if (info != null)
		{
			_guildRaidDungeonInfoDic[info.ChapterIndex] = info;
		}
	}

	public void Apply(GuildRaidDungeonInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
			{
				Apply(infos[i]);
			}
		}
	}

	public void Apply(GuildRaidMemberTotalScoreInfo info)
	{
		if (info != null)
		{
			_scoreDictionary[info.AccountId] = info;
		}
	}

	public void Apply(GuildRaidMemberTotalScoreInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public GuildRaidMemberTotalScoreInfo GetGuildRaidScoreInfo(long accountId)
	{
		_scoreDictionary.TryGetValue(accountId, out var value);
		return value;
	}

	public IEnumerable<GuildRaidMemberTotalScoreInfo> GetGuildRaidMemberScores()
	{
		return _scoreDictionary.Values;
	}

	public int GetGuildRaidScoreRank(long accountId)
	{
		GuildRaidMemberTotalScoreInfo guildRaidScoreInfo = GetGuildRaidScoreInfo(accountId);
		if (guildRaidScoreInfo == null)
		{
			return 0;
		}
		int num = 1;
		foreach (KeyValuePair<long, GuildRaidMemberTotalScoreInfo> item in _scoreDictionary)
		{
			if (accountId != item.Key && item.Value.Score > guildRaidScoreInfo.Score)
			{
				num++;
			}
		}
		return num;
	}

	public void ClearGuildRaidScore(int chapterIndex)
	{
		_scoreDictionary.Clear();
	}

	public GuildRaidDungeonInfo GetGuildRaidDungeonInfo(int chapterIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_guildRaidDungeonInfoDic))
		{
			return null;
		}
		return _guildRaidDungeonInfoDic[chapterIndex];
	}

	public bool IsBonusDungeon(out GuildRaidDungeonInfo bonusDungeonInfo)
	{
		bonusDungeonInfo = null;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_guildRaidDungeonInfoDic))
		{
			return false;
		}
		List<GuildRaidDungeonInfo> list = _guildRaidDungeonInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return false;
		}
		GuildRaidDungeonInfo guildRaidDungeonInfo = null;
		for (int i = 0; i < list.Count(); i++)
		{
			guildRaidDungeonInfo = list[i];
			if (guildRaidDungeonInfo != null && guildRaidDungeonInfo.CheckBonusDungeon)
			{
				bonusDungeonInfo = guildRaidDungeonInfo;
				return true;
			}
		}
		return false;
	}

	public bool IsDummyDungeon(out GuildRaidDungeonInfo dummyDungeonInfo)
	{
		dummyDungeonInfo = null;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_guildRaidDungeonInfoDic))
		{
			return false;
		}
		List<GuildRaidDungeonInfo> list = _guildRaidDungeonInfoDic.Values.ToList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return false;
		}
		GuildRaidDungeonInfo guildRaidDungeonInfo = null;
		for (int i = 0; i < list.Count(); i++)
		{
			guildRaidDungeonInfo = list[i];
			if (guildRaidDungeonInfo != null && guildRaidDungeonInfo.CheckDummyDungeon)
			{
				dummyDungeonInfo = guildRaidDungeonInfo;
				return true;
			}
		}
		return false;
	}

	public void SetOpenTime()
	{
		GuildRaidDungeonInfo normalGuildRaidDungeonInfo = NormalGuildRaidDungeonInfo;
		if (normalGuildRaidDungeonInfo != null)
		{
			_openTimeLocalAccount.Value = normalGuildRaidDungeonInfo.OpenedTime;
		}
	}

	public bool CheckOpenGuildRaidDungeon(GuildRaidDungeonInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(info.OpenedTime, out var o_dateTime))
		{
			return false;
		}
		double totalSeconds = (GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - o_dateTime).TotalSeconds;
		if (totalSeconds < 0.0)
		{
			return false;
		}
		if (totalSeconds > 3.0)
		{
			return false;
		}
		return true;
	}
}
