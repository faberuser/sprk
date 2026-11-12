namespace NShared.UseHeroSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public ItemUseResultInfo[] ItemUseResults { get; set; }

	public HeroAddResultInfo HeroResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public NPCFriendlyInfo FriendlyInfo { get; set; }
}
