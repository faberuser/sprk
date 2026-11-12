namespace NShared.GetTowerHeroInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/get_tower_hero_info";

	public int TowerIndex { get; set; }
}
