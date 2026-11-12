namespace NShared.UseCostumeSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_costume_select_item";

	public int ItemIndex { get; set; }

	public int CostumeIndex { get; set; }

	public bool GetOtherReward { get; set; }
}
