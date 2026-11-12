namespace NShared.GetEquipGacha;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "equip_gacha/get_equip_gacha";
}
