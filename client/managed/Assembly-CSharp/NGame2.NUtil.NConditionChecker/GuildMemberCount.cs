using System;
using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NShared.NUtil;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class GuildMemberCount : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		if (!bool.TryParse(values[1], out var result))
		{
			return false;
		}
		if (!int.TryParse(values[2], out var result2))
		{
			return false;
		}
		int num = 3;
		if (__Extension_Method_ICollection__.COUNT(values) < num + result2 * 2)
		{
			return false;
		}
		int guildMemeberCount = GetGuildMemeberCount(result);
		for (int i = 0; i < result2; i++)
		{
			CheckType valueEnum = ArgParser.GetValueEnum(values, num++, CheckType.None);
			if (valueEnum == CheckType.None)
			{
				return false;
			}
			int result3 = 0;
			if (!int.TryParse(values[num++], out result3))
			{
				return false;
			}
			if (!Condition.CheckCompareResult(valueEnum, result3, guildMemeberCount))
			{
				return false;
			}
		}
		return true;
	}

	private int GetGuildMemeberCount(bool onlineUserOnly)
	{
		Func<GuildMemberInfo, bool> func = IsOnlineAndNotMine;
		return GetGuildMemeberCount(onlineUserOnly ? func : null);
	}

	private bool IsOnlineAndNotMine(GuildMemberInfo memberInfo)
	{
		if (memberInfo == null)
		{
			return false;
		}
		if (!memberInfo.IsOnline)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserInfo == null)
		{
			return false;
		}
		long accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		if (memberInfo.AccountId == accountId)
		{
			return false;
		}
		return true;
	}

	private int GetGuildMemeberCount(Func<GuildMemberInfo, bool> func = null)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return 0;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return 0;
		}
		int num = 0;
		GuildMemberInfo[] values = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetValues();
		foreach (GuildMemberInfo i_argA in values)
		{
			if (func == null)
			{
				num++;
			}
			else if (Vespa_System_Action_Extension_Method.fire(func, i_argA))
			{
				num++;
			}
		}
		return num;
	}
}
