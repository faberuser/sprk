namespace NShared.GetTowerNpcInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/get_tower_npc_info";

	public int TowerIndex { get; set; }
}
