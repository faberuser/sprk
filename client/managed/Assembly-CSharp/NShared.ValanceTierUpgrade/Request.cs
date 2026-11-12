namespace NShared.ValanceTierUpgrade;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "valance/valance_tier_upgrade";
}
