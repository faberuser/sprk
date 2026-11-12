namespace NShared.B2M_GetGameServerList;

[BattlePacket]
public class Response
{
	public ResultType Result { get; set; }

	public string[] GameServerHosts { get; set; }

	public bool IsBlocked { get; set; }

	public bool MultithreadEnabled { get; set; }
}
