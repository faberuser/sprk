using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class GetStateDamageR : GetDamageR
{
	public override void GetInstantDamageInfo(out long amount, out DamageFlag damageFlag)
	{
		int valueInt = base.OperationInfo.GetValueInt(0);
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		DamageFlag valueFlag = base.OperationInfo.GetValueFlag<DamageFlag>(2);
		string valueString = base.OperationInfo.GetValueString(3);
		int valueInt3 = base.OperationInfo.GetValueInt(4);
		bool valueBool = base.OperationInfo.GetValueBool(5);
		int num = 0;
		if (valueBool)
		{
			num += base.OperationInfo.Creature.StateController.GetStateOfFiredCreature(valueString, base.OperationInfo.FiredCreature)?.GetStackCount() ?? 0;
		}
		else
		{
			List<IStateModifier> allStateModifier = base.OperationInfo.Creature.StateController.GetAllStateModifier(valueString);
			if (allStateModifier != null)
			{
				for (int i = 0; i < allStateModifier.Count; i++)
				{
					num += allStateModifier[i]?.GetStackCount() ?? 0;
				}
			}
		}
		int ratio = valueInt + valueInt3 * num;
		base.OperationInfo.FiredCreature.StatController.GetAttackAmountAndFlagByPower(base.OperationInfo.SkillData, ratio, out damageFlag, out amount);
		amount += valueInt2;
		damageFlag |= valueFlag;
	}
}
