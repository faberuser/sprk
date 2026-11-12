namespace NShared.PetCancelExplore;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_cancel_explore";

	public int ExploreIndex { get; set; }

	public int DeckIndex { get; set; }
}
