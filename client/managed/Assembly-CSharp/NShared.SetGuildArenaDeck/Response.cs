namespace NShared.SetGuildArenaDeck;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildArenaDeckInfo[] DeckInfos { get; set; }
}
