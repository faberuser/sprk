using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NGame2.NGuild;

public class GuildSuppressManager
{
	private GuildSuppressSessionInfo _guildSuppressSessionInfo;

	private GuildSuppressInfo _info;

	private GuildSuppressScoreInfo _scoreInfo;

	private GuildSuppressBanInfo _banInfo;

	private List<GuildSuppressPlayInfo> _playInfos = new List<GuildSuppressPlayInfo>();

	public bool Applied { get; set; }

	public bool AutoApplySuppress { get; set; }

	public void ApplySessionInfo(GuildSuppressSessionInfo info, bool applied, int autoApply)
	{
		if (info == null)
		{
			Debug.LogError("GuildSuppressSessionInfo is null");
		}
		_guildSuppressSessionInfo = info;
		Applied = applied;
		AutoApplySuppress = ((autoApply != 0) ? true : false);
	}

	public void ApplyBanInfo(GuildSuppressBanInfo info)
	{
		if (info == null)
		{
			Debug.LogError("GuildSuppressBanInfo is null");
		}
		_banInfo = info;
	}

	public void Apply(GuildSuppressInfo info)
	{
		if (info != null)
		{
			_info = info;
		}
	}

	public void Apply(GuildSuppressPlayInfo[] infos)
	{
		if (__Extension_Method_ICollection__.COUNT(infos) != 0 && _playInfos != null)
		{
			_playInfos.Clear();
			_playInfos.AddRange(infos);
		}
	}

	public void Apply(GuildSuppressPlayInfo info)
	{
		if (info != null && _playInfos != null)
		{
			int num = _playInfos.FindIndex((GuildSuppressPlayInfo playInfo) => playInfo.RaidIndex == info.RaidIndex && playInfo.Level == info.Level);
			if (num != -1)
			{
				_playInfos[num] = info;
			}
			else
			{
				_playInfos.Add(info);
			}
		}
	}

	public GuildSuppressInfo GetGuildSuppressInfo()
	{
		return _info;
	}

	public GuildSuppressBanInfo GetGuildSuppressBanInfo()
	{
		return _banInfo;
	}

	public GuildSuppressSessionInfo GetGuildSuppressSessionInfo()
	{
		return _guildSuppressSessionInfo;
	}

	public GuildSuppressPlayInfo GetGuildSuppressPlayInfo(int raidIndex, int level)
	{
		if (__Extension_Method_ICollection__.COUNT(_playInfos) == 0)
		{
			return null;
		}
		GuildSuppressPlayInfo guildSuppressPlayInfo = _playInfos.Find((GuildSuppressPlayInfo playInfo) => playInfo.RaidIndex == raidIndex && playInfo.Level == level);
		if (guildSuppressPlayInfo != null)
		{
			return guildSuppressPlayInfo;
		}
		return null;
	}

	public void Apply(GuildSuppressScoreInfo info)
	{
		if (info != null)
		{
			_scoreInfo = info;
		}
	}

	public GuildSuppressScoreInfo GetGuildSuppressScoreInfo()
	{
		return _scoreInfo;
	}
}
