namespace NShared.RecvRewardFriendlyPoint;

[Response]
public class Response : GenericResponse<ResultType>
{
	public NPCFriendlyInfo FriendlyInfo { get; set; }

	public HeroAddResultInfo[] HeroResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public ExpResultInfo[] ExpResultInfos { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }
}
