using NShared.NEvent;

namespace NGame2.NScenarioQuest.NRuntime;

public class EnterBattle : BaseWaitingEventRuntime<NShared.NEvent.EnterBattle>
{
	public override bool OnEvent(NShared.NEvent.EnterBattle triggerEvent)
	{
		return true;
	}
}
