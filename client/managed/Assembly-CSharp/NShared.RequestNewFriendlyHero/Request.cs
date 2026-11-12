namespace NShared.RequestNewFriendlyHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/request_new_friendly_hero";
}
