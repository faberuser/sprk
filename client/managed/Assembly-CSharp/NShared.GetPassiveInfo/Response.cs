namespace NShared.GetPassiveInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int[] PassiveInfos { get; set; }
}
