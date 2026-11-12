namespace NShared.ReceivePushReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResultInfos { get; set; }

	public ItemResultInfo[] ItemResultInfos { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }

	public FriendshipPointResultInfo[] FriendshipPointResultInfos { get; set; }

	public EquipItemInfo[] EquipItemResultInfos { get; set; }

	public HeroInfo[] HeroResultInfos { get; set; }

	public ExpResultInfo[] ExpResultInfos { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostResultInfos { get; set; }
}
