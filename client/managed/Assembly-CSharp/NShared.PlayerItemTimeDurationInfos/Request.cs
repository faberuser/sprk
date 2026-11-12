namespace NShared.PlayerItemTimeDurationInfos;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "reload/player_item_time_duration_infos";
}
