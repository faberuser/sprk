namespace NShared.GetPetExploreList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/get_pet_explore_list";
}
