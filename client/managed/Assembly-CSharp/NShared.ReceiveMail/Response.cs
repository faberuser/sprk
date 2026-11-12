namespace NShared.ReceiveMail;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MailReceiveResult MailReceiveResult { get; set; }
}
