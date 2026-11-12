namespace NShared.B2G_EclipsePing;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_eclipse_ping";

	public string SessionKey { get; set; }
}
