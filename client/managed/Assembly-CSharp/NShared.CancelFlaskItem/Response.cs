namespace NShared.CancelFlaskItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public FlaskResultInfo FlaskResult { get; set; }
}
