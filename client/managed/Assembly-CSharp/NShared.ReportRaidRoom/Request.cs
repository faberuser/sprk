namespace NShared.ReportRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/report_raid_room";

	public long RoomNo { get; set; }

	public int Reason { get; set; }
}
