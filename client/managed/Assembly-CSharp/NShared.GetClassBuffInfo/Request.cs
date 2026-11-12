namespace NShared.GetClassBuffInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "class_buff/get_class_buff_info";
}
