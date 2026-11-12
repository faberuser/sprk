namespace NShared;

public class OnStart : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnStart;

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		return true;
	}
}
