namespace NShared.GetFreeEquipGachaInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/get_free_equip_gacha_info";
}
