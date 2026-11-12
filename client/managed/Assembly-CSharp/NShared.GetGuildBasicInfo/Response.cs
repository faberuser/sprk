namespace NShared.GetGuildBasicInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MyGuildInfo info { get; set; }
}
