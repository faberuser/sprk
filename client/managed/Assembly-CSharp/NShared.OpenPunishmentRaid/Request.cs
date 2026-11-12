namespace NShared.OpenPunishmentRaid;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "punishment_raid/open_punishment_raid";

	public int GroupIndex { get; set; }

	public int DungeonType { get; set; }

	public int Level { get; set; }
}
