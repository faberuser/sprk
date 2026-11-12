namespace NShared.GetGuildRaidMemberScoreList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRaidMemberTotalScoreInfo[] GuildRaidMemberTotalScores { get; set; }
}
