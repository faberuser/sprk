using NGame2.NGuild;
using NVespa.NSingleton;

namespace NGame2.NContents;

public class GuildArenaMenuOpenChecker : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null || GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info == null)
		{
			return false;
		}
		return true;
	}
}
