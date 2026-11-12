namespace NShared.RecvRewardPhaseStep;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int AchievementStep { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }
}
