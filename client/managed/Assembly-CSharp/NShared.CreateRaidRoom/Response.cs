namespace NShared.CreateRaidRoom;

[Response]
public class Response : GenericResponse<ResultType>
{
	public long RoomNo { get; set; }
}
