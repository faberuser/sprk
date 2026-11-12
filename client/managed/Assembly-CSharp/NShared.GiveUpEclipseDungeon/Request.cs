namespace NShared.GiveUpEclipseDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/give_up_eclipse_dungeon";

	public int MatchIndex { get; set; }
}
