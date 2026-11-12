namespace NShared.EditAccessoryCostumePosition;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/edit_accessory_costume_position";

	public int HeroIndex { get; set; }

	public string AccessoryCostumePositionInfo { get; set; }
}
