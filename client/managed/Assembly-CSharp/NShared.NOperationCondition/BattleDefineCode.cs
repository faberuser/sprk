namespace NShared.NOperationCondition;

public class BattleDefineCode : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		if (values == null)
		{
			return false;
		}
		if (firedOperationInfo.FiredCreature.BattleInstance.BattleDefine == null)
		{
			return false;
		}
		for (int i = 0; i < values.Length; i++)
		{
			string valueString = ArgParser.GetValueString(values, i);
			HashString code = firedOperationInfo.FiredCreature.BattleInstance.BattleDefine.Code;
			if (valueString.Equals(code))
			{
				return true;
			}
		}
		return false;
	}
}
