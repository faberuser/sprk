namespace NShared.DelegatePartyDungeonRoomMaster;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/delegate_party_dungeon_room_master";

	public long RoomNo { get; set; }

	public long PrevMasterId { get; set; }

	public long CurMasterId { get; set; }
}
