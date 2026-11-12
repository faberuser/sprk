namespace NShared.LeavePartyDungeonRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/leave_party_dungeon_room";

	public long RoomNo { get; set; }

	public long AccountId { get; set; }

	public bool PartyDungeonRoomDestroyed { get; set; }
}
