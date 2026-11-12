namespace NShared.SearchRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/search_raid_room";

	public int RaidIndex { get; set; }

	public int StartRaidMonsterLevel { get; set; }

	public byte GoalIndex { get; set; }
}
