namespace NShared.GetGlobalMailList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MailInfo[] MailInfos { get; set; }
}
