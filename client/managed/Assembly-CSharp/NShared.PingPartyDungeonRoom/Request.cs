namespace NShared.PingPartyDungeonRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/ping_party_dungeon_room";

	public long RoomNo { get; set; }
}
