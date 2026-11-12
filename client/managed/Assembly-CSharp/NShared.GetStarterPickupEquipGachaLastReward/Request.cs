namespace NShared.GetStarterPickupEquipGachaLastReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/get_starter_pickup_equip_gacha_last_reward";

	public int GachaIndex { get; set; }
}
