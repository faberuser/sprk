namespace NShared.RewardAchievement;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] itemResults { get; set; }

	public EquipItemInfo[] equipItemInfos { get; set; }

	public StaminaResultInfo[] staminaResults { get; set; }

	public ExpResultInfo expResult { get; set; }

	public FriendshipPointResultInfo[] FriendshipPointResults { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }
}
