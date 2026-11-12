namespace NShared.ApiError;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long[] SuccessedSeqs { get; set; }
}
