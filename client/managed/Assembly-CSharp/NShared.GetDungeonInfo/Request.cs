namespace NShared.GetDungeonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/get_dungeon_info";

	public BattleType BattleType { get; set; }
}
