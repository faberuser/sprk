namespace NShared.ContributeGuild;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public int ContributedGem { get; set; }

	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildMemberAccActivityPointResultInfo GuildMemberAccActivityPointResult { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }
}
