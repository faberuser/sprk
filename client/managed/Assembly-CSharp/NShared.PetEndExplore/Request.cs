namespace NShared.PetEndExplore;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_end_explore";

	public int ExploreIndex { get; set; }

	public int DeckIndex { get; set; }
}
