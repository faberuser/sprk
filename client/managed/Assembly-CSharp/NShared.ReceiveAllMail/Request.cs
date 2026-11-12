namespace NShared.ReceiveAllMail;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/receive_all_mail";

	public int MaxCount { get; set; }
}
