namespace NShared;

public class OnWaveStart : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnWaveStart;

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		int value = 0;
		if (valueGetter.GetTriggerValue(firedCreature, 1, out value) && value != 0 && firedCreature.BattleInstance.Snapshot.WaveIndex != value)
		{
			return false;
		}
		return true;
	}
}
