namespace NShared.UseAccessorySelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_accessory_select_item";

	public int ItemIndex { get; set; }

	public int AccessoryCostumeIndex { get; set; }

	public string PositionInfo { get; set; }

	public int HeroIndex { get; set; }

	public bool GetOtherReward { get; set; }
}
