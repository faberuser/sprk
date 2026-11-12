namespace NShared.CheckAchievements;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "achievement/check_achievements";
}
