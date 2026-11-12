namespace NShared.PetAwaken;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_awaken";

	public int PetIndex { get; set; }
}
