namespace NShared.InspectUser;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CachedAccountInfo CachedAccountInfo { get; set; }

	public int MatchTierRank { get; set; }

	public int MatchTotalRank { get; set; }

	public GuildInfo GuildInfo { get; set; }
}
