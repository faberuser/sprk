namespace NShared.GetAchievements;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "achievement/get_achievements";
}
