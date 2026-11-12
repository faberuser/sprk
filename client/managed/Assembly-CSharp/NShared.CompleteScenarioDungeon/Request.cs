namespace NShared.CompleteScenarioDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/complete_scenario_dungeon";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }
}
