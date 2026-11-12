namespace NShared.ResetTreasureHouseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "treasure_house/reset_treasure_house_info";
}
