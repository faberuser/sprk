namespace NShared.UseBoosterItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public PlayerItemTimeDurationInfo ItemTimeDurationInfo { get; set; }
}
