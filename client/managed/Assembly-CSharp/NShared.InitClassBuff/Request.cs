namespace NShared.InitClassBuff;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "class_buff/init_class_buff";

	public int TagType { get; set; }
}
