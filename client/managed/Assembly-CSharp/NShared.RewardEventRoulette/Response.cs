namespace NShared.RewardEventRoulette;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public DbRewardResult RewardResult { get; set; }
}
