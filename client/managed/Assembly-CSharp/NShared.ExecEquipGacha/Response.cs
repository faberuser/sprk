namespace NShared.ExecEquipGacha;

[Response]
public class Response : GenericResponse<ResultType>
{
	public FreeEquipGachaInfo GachaInfo { get; set; }

	public EquipGachaInfo EquipGachaInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public GachaItemResultInfo[] GachaItemResults { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public RewardResultInfo StepUpRewardInfo { get; set; }

	public RewardResultInfo StarterPickupRewardInfo { get; set; }

	public StepUpEquipGachaInfo StepUpEquipGachaInfo { get; set; }

	public StarterPickupEquipGachaInfo StarterPickupEquipGachaInfo { get; set; }

	public PetGachaCountInfo[] PetGachaCountInfos { get; set; }

	public RewardResultInfo PetRewardInfo { get; set; }

	public GachaCeilingCountInfo[] GachaCeilingCountInfos { get; set; }

	public RewardResultInfo GachaCeilingRewardInfo { get; set; }

	public PlayerProductPurchaseInfo PlayerProductPurchaseInfo { get; set; }
}
