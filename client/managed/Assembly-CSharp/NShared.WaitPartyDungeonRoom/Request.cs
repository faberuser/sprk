namespace NShared.WaitPartyDungeonRoom;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "party_dungeon/wait_party_dungeon_room";

	public PartyDungeonType DungeonType { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PartyDungeonDifficultyType DungeonDifficulty { get; set; }
}
