namespace NShared.GetDOWDungeonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public DOWDungeonInfo[] DOWDungeonInfos { get; set; }

	public int UndergroundPrisonKey { get; set; }

	public string UndergroundPrisonKeyRechargeTime { get; set; }
}
