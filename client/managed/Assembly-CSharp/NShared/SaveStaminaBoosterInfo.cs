using System.Collections.Generic;

namespace NShared;

public class SaveStaminaBoosterInfo
{
	public List<BattleType> BattleTypeList = new List<BattleType>();

	public int SaveStaminaValue;

	public bool IsActiveBattleType(BattleType battleType)
	{
		if (BattleTypeList.Contains(battleType))
		{
			return SaveStaminaValue > 0;
		}
		return false;
	}
}
