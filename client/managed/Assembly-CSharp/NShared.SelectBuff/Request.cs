namespace NShared.SelectBuff;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/select_buff";

	public int BuffIndex { get; set; }
}
