namespace NShared.GetKingsPassSeasonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public KingsPassSeaonInfo KingsPassSeaonInfo { get; set; }

	public PlayerKingsPassExp PlayerKingsPassExpInfo { get; set; }

	public KingsPassMission[] KingsPassMissionInfos { get; set; }
}
