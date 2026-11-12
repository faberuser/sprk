using NGame2.NOrdeal;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CompleteOrdealArenaFloor : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return true;
		}
		int valueInt = ArgParser.GetValueInt(values, 1);
		int completeFloor = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetCompleteFloor();
		if (completeFloor == -1 || completeFloor < valueInt)
		{
			return true;
		}
		return false;
	}
}
