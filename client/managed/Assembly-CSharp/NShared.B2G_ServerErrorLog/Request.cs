namespace NShared.B2G_ServerErrorLog;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_server_error_log";

	public string ServerName { get; set; }

	public string PublicIp { get; set; }

	public int Port { get; set; }

	public string ServerVersion { get; set; }

	public bool IsCheckServer { get; set; }

	public string ErrorMessage { get; set; }
}
