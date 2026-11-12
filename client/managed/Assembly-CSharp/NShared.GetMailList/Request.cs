namespace NShared.GetMailList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/get_mail_list";

	public long LastMailIndex { get; set; }

	public int MaxCount { get; set; }

	public bool ForceReload { get; set; }
}
