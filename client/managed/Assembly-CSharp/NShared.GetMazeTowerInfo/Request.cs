namespace NShared.GetMazeTowerInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "maze_tower/get_maze_tower_info";
}
