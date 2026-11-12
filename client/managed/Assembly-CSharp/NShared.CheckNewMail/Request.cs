namespace NShared.CheckNewMail;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/check_new_mail";

	public long TopmostMailIndex { get; set; }
}
