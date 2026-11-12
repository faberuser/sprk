namespace NShared.UsePackageSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public ItemResultInfo RewardItemResult { get; set; }
}
