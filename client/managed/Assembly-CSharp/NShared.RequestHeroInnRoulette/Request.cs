namespace NShared.RequestHeroInnRoulette;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/request_hero_inn_roulette";

	public int RouletteIndex { get; set; }
}
