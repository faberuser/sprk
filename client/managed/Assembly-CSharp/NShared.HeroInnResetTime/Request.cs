namespace NShared.HeroInnResetTime;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/hero_inn_reset_time";
}
