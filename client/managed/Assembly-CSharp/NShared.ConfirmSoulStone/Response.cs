namespace NShared.ConfirmSoulStone;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ResultSoulStoneItemInfo { get; set; }
}
