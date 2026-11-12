namespace NShared.UsePackageItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public RewardResultInfo[] RewardResults { get; set; }
}
