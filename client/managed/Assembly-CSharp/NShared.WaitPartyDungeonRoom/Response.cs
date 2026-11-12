namespace NShared.WaitPartyDungeonRoom;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RaidRoomInfo JoinedRaidRoomInfo { get; set; }
}
