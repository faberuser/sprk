namespace NShared.EventCraftItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public DbRewardResult RewardResult { get; set; }

	public EventCraftItemInfo EventCraftItemInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
