using NShared.NEvent;

namespace NGame2.NScenarioQuest.NRuntime;

public class UsePortal : BaseWaitingEventRuntime<NShared.NEvent.UsePortal>
{
	public override bool OnEvent(NShared.NEvent.UsePortal triggerEvent)
	{
		return true;
	}
}
