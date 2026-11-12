namespace NShared.BuyPetIncubator;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "pet/buy_pet_incubator";

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }
}
