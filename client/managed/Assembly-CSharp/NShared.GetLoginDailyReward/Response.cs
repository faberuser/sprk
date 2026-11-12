namespace NShared.GetLoginDailyReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DbRewardResult LoginDailyRewardResult { get; set; }

	public LoginDailyInfo LoginDailyInfo { get; set; }
}
