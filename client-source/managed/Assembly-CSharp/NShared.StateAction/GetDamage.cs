namespace NShared.StateAction;

public class GetDamage : IStateAction
{
	void IStateAction.Run(BattleCreature creature, string triggerStateType, string[] values, IStateModifier stateModifier)
	{
		int valueInt = ArgParser.GetValueInt(values, 0);
		int valueInt2 = ArgParser.GetValueInt(values, 1);
		DamageFlag valueEnum = ArgParser.GetValueEnum(values, 2, DamageFlag.None);
		BattleCreature firedCreature = stateModifier.GetFiredCreature();
		if (firedCreature != null)
		{
			int skillIndex = stateModifier.GetSkillIndex();
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data != null)
			{
				OperationInfo operationInfo = stateModifier.GetOperationInfo();
				long operationUid = stateModifier.GetOperationUid();
				int operationNo = stateModifier.GetOperationNo();
				long attackAmountByPower = firedCreature.StatController.GetAttackAmountByPower(valueInt, valueEnum);
				attackAmountByPower += valueInt2;
				valueEnum |= DamageFlag.StateAction;
				long finalDamage = 0L;
				creature.StatController.GiveDamage(attackAmountByPower, firedCreature, operationInfo, operationUid, operationNo, data, data, 1, 1, string.Empty, ref valueEnum, out finalDamage);
			}
		}
	}
}
