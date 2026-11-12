namespace NShared.GetTreasureHouseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "treasure_house/get_treasure_house_info";
}
