namespace NShared.GetGuildArenaMemberDeck;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaMemberDeckInfo[] MemberDeckInfos { get; set; }
}
