using NGame2.NBattleContext;
using NShared;
using NVespa;

namespace NGame2.NUtil.NConditionChecker;

public class CheckLastPlayBattleType : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		BattleType valueEnum = ArgParser.GetValueEnum(values, 1, BattleType.None);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		if (!SingletonAuto<BattleContext>.isValidInstance)
		{
			return false;
		}
		BattleType lastBattleType = SingletonAuto<BattleContext>.instance.LastBattleType;
		return valueEnum == lastBattleType == valueBool;
	}
}
