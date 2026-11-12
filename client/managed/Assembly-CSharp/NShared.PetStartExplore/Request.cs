namespace NShared.PetStartExplore;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_start_explore";

	public int ExploreIndex { get; set; }

	public int DeckIndex { get; set; }

	public int[] PetIndices { get; set; }
}
