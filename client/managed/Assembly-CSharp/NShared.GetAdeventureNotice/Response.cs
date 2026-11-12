using NShared.NWorldBoss;

namespace NShared.GetAdeventureNotice;

[Response]
public class Response : GenericResponse<ResultType>
{
	public WorldBossInfo[] WorldBossInfos { get; set; }

	public GuildRaidDungeonInfo[] GuildRaidInfos { get; set; }

	public bool GuildSuppressApplied { get; set; }

	public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }

	public MatchSeasonData MatchSeasonData { get; set; }

	public MatchSeasonData BanPickSeasonData { get; set; }
}
