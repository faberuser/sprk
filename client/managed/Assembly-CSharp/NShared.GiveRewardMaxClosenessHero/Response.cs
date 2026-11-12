namespace NShared.GiveRewardMaxClosenessHero;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] itemResults { get; set; }

	public EquipItemInfo[] equipItemInfos { get; set; }

	public StaminaResultInfo[] staminaResult { get; set; }

	public FriendshipPointResultInfo[] FriendshipPointResult { get; set; }
}
