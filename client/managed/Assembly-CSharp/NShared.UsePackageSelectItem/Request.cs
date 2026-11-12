namespace NShared.UsePackageSelectItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_package_select_item";

	public int ItemIndex { get; set; }

	public int ItemCount { get; set; }

	public int SelectItemIndex { get; set; }
}
