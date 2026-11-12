namespace NShared.UsePackageItem;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/use_package_item";

	public int ItemIndex { get; set; }

	public int ItemCount { get; set; }
}
