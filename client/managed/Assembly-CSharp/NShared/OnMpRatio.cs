namespace NShared;

public class OnMpRatio : BaseSkillTrigger
{
	public override SkillTriggerType TriggerType => SkillTriggerType.OnMpRatio;

	public int MpRatioM { get; set; }

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
				if (MpRatioM >= value)
				{
					return MpRatioM <= value2;
				}
				return false;
			}
			return MpRatioM == value;
		}
		return false;
	}
}
