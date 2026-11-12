namespace NShared.CreateRaidRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/create_raid_room";

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int RaidMonsterLevel { get; set; }

	public byte GoalIndex { get; set; }

	public byte Opened { get; set; }

	public int DungeonDifficulty { get; set; }

	public int[] AffixIndices { get; set; }
}
