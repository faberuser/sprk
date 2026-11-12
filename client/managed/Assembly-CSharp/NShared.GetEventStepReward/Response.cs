namespace NShared.GetEventStepReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int RewardStep { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }
}
