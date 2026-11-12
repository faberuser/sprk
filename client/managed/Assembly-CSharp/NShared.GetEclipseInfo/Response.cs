namespace NShared.GetEclipseInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EclipseInfo EclipseInfo { get; set; }
}
