using NShared.NEvent;

namespace NGame2.NScenarioQuest.NRuntime;

public class BattleEnd : BaseWaitingEventRuntime<NShared.NEvent.BattleEnd>
{
	public override bool OnEvent(NShared.NEvent.BattleEnd triggerEvent)
	{
		return true;
	}
}
