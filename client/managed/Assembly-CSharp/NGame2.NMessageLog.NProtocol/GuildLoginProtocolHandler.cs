using NGame2.NGuild;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildLoginProtocolHandler : MessageProtocolHandler<GuildLogin>
{
	public override void Run(long senderId, GuildLogin protocolData)
	{
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			guildManager.MemberManager?.SetOnline(senderId, isOnline: true);
		}
	}
}
