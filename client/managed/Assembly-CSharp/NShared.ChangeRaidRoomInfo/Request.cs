namespace NShared.ChangeRaidRoomInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "raid/change_raid_room_info";

	public long RoomNo { get; set; }

	public int RaidLevel { get; set; }

	public int RaidMonsterLevel { get; set; }

	public byte GoalIndex { get; set; }

	public byte Status { get; set; }

	public int[] AffixIndices { get; set; }
}
