using NGame2.NBattleContext;
using NGame2.NScene;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class LastDungeonResult : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (!(GlobalAutoBehaviour<LoadingManager>.instance.LastSceneData is BattleContext.LastDungeonResultData lastDungeonResultData))
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(values) < 5)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result))
		{
			return false;
		}
		if (!int.TryParse(values[2], out var result2))
		{
			return false;
		}
		if (!bool.TryParse(values[3], out var result3))
		{
			return false;
		}
		if (!bool.TryParse(values[4], out var result4))
		{
			return false;
		}
		if (lastDungeonResultData.EnterDungeon.Chapter == result && lastDungeonResultData.EnterDungeon.LinkedDungeon == result2 && lastDungeonResultData.EnterDungeon.IsScenario == result3)
		{
			return lastDungeonResultData.IsWin == result4;
		}
		return false;
	}
}
