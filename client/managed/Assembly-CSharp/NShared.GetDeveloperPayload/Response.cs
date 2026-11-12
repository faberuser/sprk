namespace NShared.GetDeveloperPayload;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string DeveloperPayload { get; set; }

	public EPurchasePlatform Platform { get; set; }
}
