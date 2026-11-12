namespace NShared.B2M_Ping;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2m_ping";

	public string BattleServerName { get; set; }

	public string IntegrityType { get; set; }

	public string Address { get; set; }

	public string PublicIp { get; set; }

	public string PrivateIp { get; set; }

	public int Port { get; set; }

	public int Weight { get; set; }

	public string BattleVersion { get; set; }

	public string BattleMinVersion { get; set; }

	public string BattleMaxVersion { get; set; }

	public bool SensitiveDebugging { get; set; }

	public int CpuUsage { get; set; }

	public bool IsCheckServer { get; set; }

	public bool IsCheckSimulationServer { get; set; }

	public string[] IntegrityRetRequestIds { get; set; }

	public long[] IntegrityRetAccountIds { get; set; }

	public string[] IntegrityRetResults { get; set; }

	public string[] IntegrityRetExactResult { get; set; }

	public long[] IntegrityRetClientTotalDamage { get; set; }
}
