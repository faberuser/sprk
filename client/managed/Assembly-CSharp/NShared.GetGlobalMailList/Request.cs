namespace NShared.GetGlobalMailList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "mail/get_global_mail_list";
}
