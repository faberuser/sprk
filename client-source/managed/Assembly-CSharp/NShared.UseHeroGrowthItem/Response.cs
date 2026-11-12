namespace NShared.UseHeroGrowthItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public ItemUseResultInfo[] ItemUseResults { get; set; }

	public HeroInfo HeroResult { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public NPCFriendlyInfo FriendlyInfo { get; set; }

	public PlayerHeroFriendlyInfo HeroFriendlyInfo { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }
}
