namespace NShared.ChangePartyDungeonRoomInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/change_party_dungeon_room_info";

	public long RoomNo { get; set; }

	public PartyDungeonType DungeonType { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PartyDungeonDifficultyType DungeonDifficulty { get; set; }

	public byte Status { get; set; }
}
