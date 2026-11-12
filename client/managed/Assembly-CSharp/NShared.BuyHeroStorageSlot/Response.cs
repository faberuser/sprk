namespace NShared.BuyHeroStorageSlot;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroPresetStorageInfo HeroPresetStorage { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
