namespace NShared.PetIncubatorSlotExpansion;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/pet_incubator_slot_expansion";
}
