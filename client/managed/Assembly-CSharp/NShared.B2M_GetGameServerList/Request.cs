namespace NShared.B2M_GetGameServerList;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2m_get_gameserver_list";

	public string BattleServerName { get; set; }

	public string PublicIp { get; set; }

	public string PrivateIp { get; set; }

	public int Port { get; set; }

	public int SessionCu { get; set; }

	public int RealtimeCu { get; set; }

	public string ServerVersion { get; set; }

	public int RunningTime { get; set; }

	public int CpuUsage { get; set; }

	public int FreeMem { get; set; }

	public int FreeMemPercentage { get; set; }
}
