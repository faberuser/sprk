namespace NShared.GetJoinRequestPlayers;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildJoinRequestPlayerInfo[] players { get; set; }
}
