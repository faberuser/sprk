namespace NShared.GiveGiftItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public NPCFriendlyInfo FriendlyInfo { get; set; }

	public EventGiftPointResultInfo EventGiftPointResult { get; set; }
}
