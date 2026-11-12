namespace NShared.NOperationCondition;

public class Or : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		int num = 100;
		for (int i = 0; i < num; i++)
		{
			int valueInt = ArgParser.GetValueInt(values, i * 2);
			int valueInt2 = ArgParser.GetValueInt(values, i * 2 + 1);
			if (valueInt == 0)
			{
				break;
			}
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(valueInt);
			if (data == null)
			{
				break;
			}
			OperationData operationData = data.GetOperationData(valueInt2);
			if (operationData != null)
			{
				OperationConditionType conditionType = operationData.ConditionType;
				bool reverseCheck = operationData.ReverseCheck;
				string[] conditionValues = operationData.ConditionValues;
				if (GameRoot.Instance.OperationConditionFactory.MeetCondition(conditionType, firedOperationInfo, targetCreature, conditionValues, reverseCheck))
				{
					return true;
				}
			}
		}
		return false;
	}
}
