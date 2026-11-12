namespace NShared.GetUnderPrisonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public UnderPrisonInfo[] UnderPrisonInfos { get; set; }

	public int UnderPrisonKey { get; set; }

	public string UnderPrisonKeyRechargeTime { get; set; }
}
