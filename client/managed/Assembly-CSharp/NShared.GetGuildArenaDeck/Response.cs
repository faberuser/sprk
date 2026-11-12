namespace NShared.GetGuildArenaDeck;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaDeckInfo[] DeckInfos { get; set; }
}
