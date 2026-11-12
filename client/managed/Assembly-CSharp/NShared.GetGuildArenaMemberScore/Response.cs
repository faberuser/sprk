namespace NShared.GetGuildArenaMemberScore;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaMemberScoreInfo MyMemberScore { get; set; }

	public GuildArenaMemberScoreInfo[] MemberScores { get; set; }
}
