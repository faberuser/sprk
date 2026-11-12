namespace NShared.B2G_GetEclipseInfo;

[BattlePacket]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/b2g_get_eclipse_info";

	public long AccountId { get; set; }
}
