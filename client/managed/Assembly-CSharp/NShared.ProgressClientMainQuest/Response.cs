namespace NShared.ProgressClientMainQuest;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int Progress { get; set; }
}
