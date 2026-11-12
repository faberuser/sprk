namespace NShared.GetSoulStoneMileageReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip/get_soul_stone_mileage_reward";

	public int SoulStoneRestoreIndex { get; set; }
}
