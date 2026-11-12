using NGame2.NGuild;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildLogoutProtocolHandler : MessageProtocolHandler<GuildLogout>
{
	public override void Run(long senderId, GuildLogout protocolData)
	{
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			guildManager.MemberManager?.SetOnline(senderId, isOnline: false);
		}
	}
}
