using NGame2.NGuild;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class JoinGuild : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 2)
		{
			return false;
		}
		if (!bool.TryParse(values[1], out var result))
		{
			return false;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id <= 0)
		{
			return !result;
		}
		return result;
	}
}
