namespace NShared.ReceiveMail;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/receive_mail";

	public long MailIndex { get; set; }
}
