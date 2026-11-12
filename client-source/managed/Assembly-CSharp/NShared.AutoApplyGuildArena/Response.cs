namespace NShared.AutoApplyGuildArena;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int AutoApply { get; set; }
}
