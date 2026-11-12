namespace NShared.GetCertificate;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string Certificate { get; set; }

	public long Salt { get; set; }

	public string PublicIp { get; set; }
}
