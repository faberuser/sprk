namespace NShared.ResetTower;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/reset_tower";

	public int TowerIndex { get; set; }
}
