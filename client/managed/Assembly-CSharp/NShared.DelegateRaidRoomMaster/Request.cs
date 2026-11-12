namespace NShared.DelegateRaidRoomMaster;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/delegate_raid_room_master";

	public long RoomNo { get; set; }

	public long PrevMasterId { get; set; }

	public long CurMasterId { get; set; }
}
