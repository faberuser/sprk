namespace NShared.PetHappyGift;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_happy_gift";

	public int PetIndex { get; set; }
}
