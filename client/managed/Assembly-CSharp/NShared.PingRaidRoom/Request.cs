namespace NShared.PingRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/ping_raid_room";

	public long RoomNo { get; set; }
}
