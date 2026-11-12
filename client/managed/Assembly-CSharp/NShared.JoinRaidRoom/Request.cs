namespace NShared.JoinRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/join_raid_room";

	public long RoomNo { get; set; }
}
