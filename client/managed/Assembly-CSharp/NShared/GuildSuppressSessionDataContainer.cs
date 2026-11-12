using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GuildSuppressSessionDataContainer : JitDataContainerDefault<int, GuildSuppressSessionData>
{
	public const string TABLENAME = "GuildSuppressSessionTable";

	private JitDictionary<int, int> _map_of_MultiToSingleLevel = new JitDictionary<int, int>();

	private JitDictionary<int, int> _map_of_SingleToMultiLevel = new JitDictionary<int, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressSessionTable");

	public GuildSuppressSessionDataContainer()
	{
		base.Packer.RegisterAndBind(_map_of_MultiToSingleLevel);
		base.Packer.RegisterAndBind(_map_of_SingleToMultiLevel);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GuildSuppressSessionData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.Level == null || item.SingleLevel == null || item.Level.Length == 0 || item.SingleLevel.Length == 0 || item.Level.Length != item.SingleLevel.Length)
			{
				continue;
			}
			for (int i = 0; i < item.Level.Length; i++)
			{
				int num = item.Level[i];
				int num2 = item.SingleLevel[i];
				if (!_map_of_MultiToSingleLevel.JsonLoad_Get().ContainsKey(num))
				{
					_map_of_MultiToSingleLevel.JsonLoad_Get().Add(num, num2);
				}
				if (!_map_of_SingleToMultiLevel.JsonLoad_Get().ContainsKey(num2))
				{
					_map_of_SingleToMultiLevel.JsonLoad_Get().Add(num2, num);
				}
			}
		}
	}

	public List<GuildSuppressSessionData> GetSessionList(int seasonIndex)
	{
		List<GuildSuppressSessionData> list = new List<GuildSuppressSessionData>();
		foreach (GuildSuppressSessionData allDatum in GetAllData())
		{
			if (allDatum.SeasonIndex == seasonIndex)
			{
				list.Add(allDatum);
			}
		}
		return RestoredPortal.ConquestSessions(list, seasonIndex);
	}

	public int GetDungeonIndexByLevel(int sessionIndex, int level)
	{
		GuildSuppressSessionData guildSuppressSessionData = RestoredPortal.ConquestSession(this, sessionIndex);
		if (guildSuppressSessionData.SuppressDungeonIndex.Length == 0 || guildSuppressSessionData.SuppressDungeonIndex.Length < level || level < 1)
		{
			return 0;
		}
		return guildSuppressSessionData.SuppressDungeonIndex[level - 1];
	}

	public int GetSingleLevel(int level)
	{
		return _map_of_MultiToSingleLevel.GetOrDefault(level, 0);
	}

	public int GetMultiLevel(int singleLevel)
	{
		return _map_of_SingleToMultiLevel.GetOrDefault(singleLevel, 0);
	}

	public GuildSuppressSessionData GetLastSessionData(int seasonIndex)
	{
		List<GuildSuppressSessionData> sessionList = GetSessionList(seasonIndex);
		if (sessionList == null || sessionList.Count == 0)
		{
			return null;
		}
		GuildSuppressSessionData guildSuppressSessionData = null;
		foreach (GuildSuppressSessionData item in sessionList)
		{
			if (guildSuppressSessionData == null)
			{
				guildSuppressSessionData = item;
			}
			else if (item.SessionIndex > guildSuppressSessionData.SessionIndex)
			{
				guildSuppressSessionData = item;
			}
		}
		return guildSuppressSessionData;
	}
}
