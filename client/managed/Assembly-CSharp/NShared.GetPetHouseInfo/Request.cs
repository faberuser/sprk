namespace NShared.GetPetHouseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/get_pet_house_info";
}
