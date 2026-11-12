using NShared;
using NShared.NEvent;

namespace NGame2.NScenarioQuest.NRuntime;

public class PlayerVisitNode : BaseWaitingEventRuntime<NShared.NEvent.PlayerVisitNode>
{
	public override bool OnEvent(NShared.NEvent.PlayerVisitNode triggerEvent)
	{
		if (base.StateValue == null || base.StateValue.Length == 0)
		{
			return true;
		}
		if (ArgParser.GetValueInt(base.StateValue, 0) == triggerEvent.ChapterIndex && ArgParser.GetValueInt(base.StateValue, 1) == triggerEvent.DungeonIndex)
		{
			return true;
		}
		return false;
	}
}
