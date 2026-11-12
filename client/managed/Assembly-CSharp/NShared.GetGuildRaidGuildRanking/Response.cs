namespace NShared.GetGuildRaidGuildRanking;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRankingInfo[] RankingInfos { get; set; }
}
