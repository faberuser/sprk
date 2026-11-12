using NShared.NEvent;

namespace NGame2.NScenarioQuest.NRuntime;

public class CompleteTutorial : BaseWaitingEventRuntime<NShared.NEvent.CompleteTutorial>
{
	public override bool OnEvent(NShared.NEvent.CompleteTutorial triggerEvent)
	{
		return true;
	}
}
