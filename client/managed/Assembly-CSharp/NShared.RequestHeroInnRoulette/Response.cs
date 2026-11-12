namespace NShared.RequestHeroInnRoulette;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RouletteInfo RouletteInfo { get; set; }

	public RouletteRewardInfo[] RouletteRewardInfo { get; set; }
}
