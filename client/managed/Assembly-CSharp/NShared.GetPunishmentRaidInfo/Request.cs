namespace NShared.GetPunishmentRaidInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "punishment_raid/get_punishment_raid_info";
}
