namespace NShared.UseMultipleHeroSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public ItemUseResultInfo[] ItemUseResults { get; set; }

	public HeroAddResultInfo[] HeroResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
