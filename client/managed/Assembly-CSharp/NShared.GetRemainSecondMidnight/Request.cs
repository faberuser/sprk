namespace NShared.GetRemainSecondMidnight;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "time/get_remain_second_midnight";
}
