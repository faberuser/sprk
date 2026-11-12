namespace NShared.AutoApplyGuildSuppress;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int AutoApplySuppress { get; set; }
}
