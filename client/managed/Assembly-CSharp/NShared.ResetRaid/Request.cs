namespace NShared.ResetRaid;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/reset_raid";

	public int RaidIndex { get; set; }
}
