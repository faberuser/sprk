using NShared.NJit;

namespace NShared;

public class GuildArenaSeasonDataContainer : JitDataContainerDefault<int, GuildArenaSeasonData>
{
	public const string TABLENAME = "GuildArenaSeasonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildArenaSeasonTable");

	public GuildArenaSeasonData GetDataBySeasonIndex(int seasonIndex)
	{
		foreach (GuildArenaSeasonData allDatum in GetAllData())
		{
			if (allDatum.SeasonIndex == seasonIndex)
			{
				return allDatum;
			}
		}
		return null;
	}

	public string GetDisplayNameBySeasonIndex(int seasonIndex)
	{
		foreach (GuildArenaSeasonData allDatum in GetAllData())
		{
			if (allDatum.SeasonIndex == seasonIndex && allDatum.DisplaySeasonText != string.Empty)
			{
				return allDatum.DisplaySeasonText;
			}
		}
		return string.Empty;
	}

	public string GetDisplayLocalKeyBySeasonIndex(int seasonIndex)
	{
		foreach (GuildArenaSeasonData allDatum in GetAllData())
		{
			if (allDatum.SeasonIndex == seasonIndex && allDatum.DisplaySeasonLocalKey != null)
			{
				return allDatum.DisplaySeasonLocalKey;
			}
		}
		return null;
	}
}
