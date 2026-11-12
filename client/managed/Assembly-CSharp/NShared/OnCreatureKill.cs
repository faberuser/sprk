namespace NShared;

public class OnCreatureKill : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnCreatureKill;

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		BattleCreature battleCreature = ((base.TargetCreatures.Length != 0) ? base.TargetCreatures[0] : null);
		if (battleCreature == null)
		{
			return false;
		}
		string value = null;
		if (valueGetter.GetTriggerValue(firedCreature, 1, out value) && !string.IsNullOrEmpty(value) && !battleCreature.StateController.IsInState(value))
		{
			return false;
		}
		return true;
	}
}
