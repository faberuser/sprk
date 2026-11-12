using System;
using System.Collections.Generic;

namespace NShared.NOperationRuntime;

public class ModDamageByState : BaseOperationRuntime, IDamageModifier, IBaseDamageModifier, ISnapshotObject
{
	protected override void OnBegin()
	{
		base.OnBegin();
		base.OperationInfo.Creature.StatController.AddDamageModifier(this);
	}

	protected override void OnEnd()
	{
		base.OnEnd();
		base.OperationInfo.Creature.StatController.RemoveDamageModifier(this);
	}

	bool IDamageModifier.Modify(BattleCreature attacker, ref long amount, ref DamageFlag damageFlag)
	{
		if ((damageFlag & DamageFlag.DerivedDamage) != DamageFlag.None)
		{
			return false;
		}
		string stateGroup = base.OperationInfo.GetValueString(0);
		int valueInt = base.OperationInfo.GetValueInt(1);
		int valueInt2 = base.OperationInfo.GetValueInt(2);
		bool valueBool = base.OperationInfo.GetValueBool(3);
		int num = 0;
		if (valueBool)
		{
			IList<IStateModifier> allStateModifier = base.OperationInfo.Creature.StateController.GetAllStateModifier((IStateModifier e) => e.GetStateData().Group == stateGroup);
			if (allStateModifier != null && allStateModifier.Count > 0)
			{
				for (int num2 = 0; num2 < allStateModifier.Count; num2++)
				{
					num += allStateModifier[num2].GetStackCount();
				}
			}
		}
		else
		{
			num += base.OperationInfo.Creature.StateController.GetGroupStateOfFiredCreature(stateGroup, base.OperationInfo.Creature)?.GetStackCount() ?? 0;
		}
		if (valueInt2 > 0)
		{
			num = Math.Min(num, valueInt2);
		}
		if (num <= 0)
		{
			return false;
		}
		long num3 = amount * valueInt * num / 1000;
		amount += num3;
		return true;
	}

	DamageModifiedType IDamageModifier.GetModifiedType()
	{
		return DamageModifiedType.State;
	}

	DamagePipelinePosition IDamageModifier.GetPipelinePosition()
	{
		return DamagePipelinePosition.Final;
	}

	long IBaseDamageModifier.GetOperationUid()
	{
		return base.OperationInfo.Uid;
	}

	bool IBaseDamageModifier.IsValid()
	{
		return true;
	}
}
