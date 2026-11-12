namespace NShared.CreatePartyDungeonRoom;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long RoomNo { get; set; }
}
