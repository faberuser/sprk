using NGame2.NBattleField.NManager.NPlay;
using NShared;

public static class ExtensionMethod_Game2_BattleField_Manager_Play_IDataProvider
{
	public static bool CanRetryBattle(this IDataProvider This, BattleRetryConditionType conditionType)
	{
		if (SprkSoloRetry.Allow(This, conditionType))
		{
			return true;
		}
		if (This == null)
		{
			return false;
		}
		if ((This.GetBattleMenuType() & EBattleMenu.Retry) != EBattleMenu.Retry)
		{
			return false;
		}
		BattleDefineData battleDefine = This.GetBattleDefine();
		if (battleDefine == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(battleDefine.RetryCondition))
		{
			return false;
		}
		return battleDefine.RetryCondition.Contains(conditionType);
	}
}
