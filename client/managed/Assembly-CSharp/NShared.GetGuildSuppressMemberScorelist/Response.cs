namespace NShared.GetGuildSuppressMemberScorelist;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildSuppressMemberScoreInfo[] GuildSuppressMemberScores { get; set; }
}
