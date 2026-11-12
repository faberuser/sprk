namespace NGame2.NUtil.NConditionChecker;

public class LoginDailyCount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !bool.TryParse(values[2], out var result2))
		{
			return false;
		}
		bool flag = false;
		if (100 >= result)
		{
			flag = true;
		}
		if (flag == result2)
		{
			return true;
		}
		return false;
	}
}
