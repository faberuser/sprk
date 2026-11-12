using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CreateAccount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		int result = 0;
		if (!int.TryParse(values[1], out result) || String_Extension_Method.IsNullOrEmpty(values[2]))
		{
			return false;
		}
		string text = values[2];
		int num = 0;
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<AccountManager>.instance.CreatedTime, out var o_dateTime))
		{
			num = (GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - o_dateTime).Days;
			switch (text)
			{
			case "Equal":
				if (num == result)
				{
					return true;
				}
				break;
			case "Up":
				if (num >= result)
				{
					return true;
				}
				break;
			case "Down":
				if (num <= result)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}
}
