namespace NShared.LevelUpClassBuff;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ClassBuffPointInfo[] ClassBuffPointInfos { get; set; }

	public ClassBuffInfo[] ClassBuffInfos { get; set; }
}
