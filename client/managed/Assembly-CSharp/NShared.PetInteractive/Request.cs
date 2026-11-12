namespace NShared.PetInteractive;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_interactive";

	public int PetIndex { get; set; }

	public PetActionType ActionType { get; set; }

	public int ActionCount { get; set; }
}
