namespace NShared.GetMailListByPage;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MailInfo[] MailInfos { get; set; }

	public int TotalCount { get; set; }
}
