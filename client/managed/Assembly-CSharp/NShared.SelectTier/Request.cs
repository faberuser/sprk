namespace NShared.SelectTier;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/select_tier";

	public MatchTierType Tier { get; set; }
}
