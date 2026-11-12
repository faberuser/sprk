namespace NShared.NOperationCondition;

public class StaminaType : IOperationCondition
{
	bool IOperationCondition.Meet(FiredOperationInfo firedOperationInfo, BattleCreature targetCreature, string[] values)
	{
		NShared.StaminaType valueEnum = ArgParser.GetValueEnum(values, 0, NShared.StaminaType.None);
		CampaignDungeonData dungeonData = firedOperationInfo.FiredCreature.BattleInstance.DungeonData;
		if (dungeonData == null)
		{
			return false;
		}
		if (dungeonData.ReqStaminaType != valueEnum)
		{
			return false;
		}
		if (dungeonData.ReqStamina <= 0)
		{
			return false;
		}
		return true;
	}
}
