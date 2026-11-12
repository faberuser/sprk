namespace NShared.BeginTutorial;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "tutorial/begin_tutorial";

	public int TutorialIndex { get; set; }
}
