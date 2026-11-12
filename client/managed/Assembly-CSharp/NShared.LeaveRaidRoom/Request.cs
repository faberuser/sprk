namespace NShared.LeaveRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/leave_raid_room";

	public long RoomNo { get; set; }

	public long AccountId { get; set; }

	public bool RaidRoomDestroyed { get; set; }
}
