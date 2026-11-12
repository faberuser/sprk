namespace NShared.RequestNewFriendlyHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerHeroFriendlyInfo FriendlyInfo { get; set; }

	public int[] MaxClosenessHeroIndice { get; set; }
}
