namespace NShared.UnsetEggInPetIncubator;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/unset_egg_in_pet_incubator";

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }
}
