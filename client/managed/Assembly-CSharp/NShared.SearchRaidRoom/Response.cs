namespace NShared.SearchRaidRoom;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RaidRoomInfo[] RoomInfos { get; set; }
}
