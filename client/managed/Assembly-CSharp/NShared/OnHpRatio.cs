namespace NShared;

public class OnHpRatio : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnHpRatio;

	public int HpRatioM { get; set; }

	public override bool CheckTriggerValue(BattleCreature firedCreature, ITriggerValueGetter valueGetter)
	{
		if (!base.CheckTriggerValue(firedCreature, valueGetter))
		{
			return false;
		}
		int value = 0;
		if (valueGetter.GetTriggerValue(firedCreature, 1, out value))
		{
			int value2 = 0;
			if (valueGetter.GetTriggerValue(firedCreature, 2, out value2))
			{
				if (HpRatioM >= value)
				{
					return HpRatioM <= value2;
				}
				return false;
			}
			return HpRatioM == value;
		}
		return false;
	}
}
