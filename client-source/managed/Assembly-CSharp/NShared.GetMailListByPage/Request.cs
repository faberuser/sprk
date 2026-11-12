namespace NShared.GetMailListByPage;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/get_mail_list_by_page";

	public int PageNo { get; set; }

	public int MaxCount { get; set; }

	public bool ForceReload { get; set; }
}
