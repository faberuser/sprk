namespace NShared.CompleteTutorial;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "tutorial/complete_tutorial";

	public int TutorialIndex { get; set; }
}
