namespace NShared.ReceiveAllMail;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MailReceiveResult[] MailReceiveResults { get; set; }

	public int ReceivedMailCount { get; set; }
}
