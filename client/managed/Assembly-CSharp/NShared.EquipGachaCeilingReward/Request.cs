namespace NShared.EquipGachaCeilingReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/equip_gacha_ceiling_reward";

	public int GachaIndex { get; set; }
}
