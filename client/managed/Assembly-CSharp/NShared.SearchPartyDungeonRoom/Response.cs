namespace NShared.SearchPartyDungeonRoom;

[Response]
public class Response : GenericResponse<ResultType>, IResponseSearchRoom
{
	public int RepeatCoolTime { get; set; }

	public RaidRoomInfo[] RoomInfos { get; set; }
}
