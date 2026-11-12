namespace NShared.ChangePetAvatar;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/change_pet_avatar";

	public int AvatarPetIndex { get; set; }

	public int PetIndex { get; set; }
}
