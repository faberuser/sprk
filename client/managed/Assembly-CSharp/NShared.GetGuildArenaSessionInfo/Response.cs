namespace NShared.GetGuildArenaSessionInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaSessionScoreInfo[] MySessionScoreInfo;

	public int SeasonIndex { get; set; }

	public GuildArenaSessionInfo SessionInfo { get; set; }

	public bool GuildArenaApplied { get; set; }

	public int AutoApply { get; set; }

	public GuildArenaRankerInfo[] Rankers { get; set; }

	public GuildMemberServerInfo MVPMember { get; set; }

	public int AddSeasonScore { get; set; }

	public int WinResult { get; set; }

	public bool IsMatched { get; set; }

	public GuildArenaSeasonType SeasonType { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
