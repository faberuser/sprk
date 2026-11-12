namespace NShared.SetEggInPetIncubator;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/set_egg_in_pet_incubator";

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }

	public int ItemIndex { get; set; }
}
