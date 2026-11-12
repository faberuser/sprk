namespace NShared.OpenGodkingTrialDungeon;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "godking_trial/open_godking_trial_dungeon";

	public int ChapterIndex { get; set; }
}
