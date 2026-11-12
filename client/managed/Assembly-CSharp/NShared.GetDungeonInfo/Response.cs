namespace NShared.GetDungeonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HideoutDungeonInfo[] DungeonInfos { get; set; }

	public ConquestDungeonInfo[] ConquestDungeonInfo { get; set; }
}
