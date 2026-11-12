namespace NShared.GiveRewardHeroInnRoulette;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RouletteInfo RouletteInfo { get; set; }

	public int Section { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] itemResults { get; set; }

	public EquipItemInfo[] equipItemInfos { get; set; }

	public StaminaResultInfo staminaResult { get; set; }

	public ExpResultInfo expResult { get; set; }

	public StaminaResultInfo EventDungeonTicket { get; set; }

	public FriendshipPointResultInfo FriendshipPointResult { get; set; }
}
