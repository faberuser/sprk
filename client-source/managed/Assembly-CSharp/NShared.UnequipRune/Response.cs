namespace NShared.UnequipRune;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public HeroRunePageInfo HeroRunePageInfo { get; set; }
}
