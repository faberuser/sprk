namespace NShared.DoHeroFriendly;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerHeroFriendlyInfo FriendlyInfo { get; set; }

	public HeroInfo HeroInfo { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public FriendshipPointResultInfo FriendshipPointResult { get; set; }
}
