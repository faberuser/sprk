namespace NShared.GetKeyDecoder;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string key_param { get; set; }
}
