namespace NShared.GetRemainSecondMidnight;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int RemainSecond { get; set; }
}
