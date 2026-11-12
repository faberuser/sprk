namespace NShared.UsePetSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_pet_select_item";

	public int ItemIndex { get; set; }

	public int PetIndex { get; set; }
}
