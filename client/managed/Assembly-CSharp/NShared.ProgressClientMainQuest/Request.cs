namespace NShared.ProgressClientMainQuest;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "quest/progress_client_main_quest";
}
