namespace NShared.GetKingsPassSeasonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "kings_pass/get_kings_pass_season_info";
}
