namespace NShared.GetPetMiscInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/get_pet_misc_info";
}
