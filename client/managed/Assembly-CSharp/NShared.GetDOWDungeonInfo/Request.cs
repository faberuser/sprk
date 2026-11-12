namespace NShared.GetDOWDungeonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "dow_dungeon/get_dow_dungeon_info";
}
