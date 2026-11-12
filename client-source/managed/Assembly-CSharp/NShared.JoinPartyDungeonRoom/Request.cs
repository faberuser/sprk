namespace NShared.JoinPartyDungeonRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/join_party_dungeon_room";

	public long RoomNo { get; set; }
}
