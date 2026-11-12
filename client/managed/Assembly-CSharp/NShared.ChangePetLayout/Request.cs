namespace NShared.ChangePetLayout;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/change_pet_layout";

	public int[] PetIndices { get; set; }
}
