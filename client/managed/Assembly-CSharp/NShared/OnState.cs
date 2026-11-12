using System;

namespace NShared;

public class OnState : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnState;

	public string StateType { get; set; }

	public string StateGroup { get; set; }

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		string value = null;
		if (valueGetter.GetTriggerValue(firedCreature, 1, out value) && !string.IsNullOrEmpty(value) && !string.Equals(StateType, value, StringComparison.OrdinalIgnoreCase) && !string.Equals(StateGroup, value, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}
}
