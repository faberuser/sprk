namespace NShared.GetDeveloperPayload;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/get_developer_payload";

	public EPurchasePlatform Platform { get; set; }

	public string ProductID { get; set; }

	public int ProductIndex { get; set; }
}
