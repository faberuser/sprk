namespace NShared.ResetPunishmentRaid;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "punishment_raid/reset_punishment_raid";

	public int GroupIndex { get; set; }

	public int DungeonType { get; set; }
}
