namespace NShared.GetGuildSuppressSessionInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildSuppressSessionInfo GuildSuppressSessionInfo { get; set; }

	public GuildSuppressSeasonInfo GuildSuppressSeasonInfo { get; set; }

	public bool GuildSuppressApplied { get; set; }

	public GuildSuppressInfo GuildSuppressInfo { get; set; }

	public GuildSuppressPlayInfo[] GuildSuppressPlayInfos { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public int AutoApplySuppress { get; set; }

	public GuildSuppressBanInfo GuildSuppressBanInfo { get; set; }
}
