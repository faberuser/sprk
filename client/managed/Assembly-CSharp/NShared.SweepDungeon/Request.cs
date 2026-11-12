namespace NShared.SweepDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "sweep/sweep_dungeon";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public string[] SelectedRewardItemCodes { get; set; }

	public long SweepCount { get; set; }
}
