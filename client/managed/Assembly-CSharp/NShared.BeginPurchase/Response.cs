namespace NShared.BeginPurchase;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string DeveloperPayload { get; set; }

	public string ProductId { get; set; }

	public EPurchasePlatform Platform { get; set; }
}
