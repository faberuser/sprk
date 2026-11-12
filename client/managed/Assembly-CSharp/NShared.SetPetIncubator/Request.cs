namespace NShared.SetPetIncubator;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/set_pet_incubator";

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }
}
