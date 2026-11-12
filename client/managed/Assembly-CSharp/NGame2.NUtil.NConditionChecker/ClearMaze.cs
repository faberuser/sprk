using NGame2.NAccount;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class ClearMaze : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		int count = 0;
		int minutes = 0;
		ConditionChecker instance = SingletonAuto<ConditionChecker>.instance;
		if (instance == null)
		{
			return false;
		}
		if (!instance.CheckGetValues(values, ref count, ref minutes))
		{
			return false;
		}
		CheckType checkType = CheckType.None;
		CheckType checkType2 = CheckType.None;
		if (!SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[2], ref checkType) || !SingletonAuto<ConditionChecker>.instance.CheckGetCheckType(values[4], ref checkType2))
		{
			return false;
		}
		if (!instance.CheckCount(StaminaType.MazeKey, count, checkType) || !CheckMazeProgressTime())
		{
			return false;
		}
		return true;
	}

	private bool CheckMazeProgressTime()
	{
		int day = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime().Day;
		if (day >= 1 && day <= 15)
		{
			return true;
		}
		return false;
	}
}
