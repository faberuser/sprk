using NGame2.NChatting;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidOpenHandler : MessageProtocolHandler<GuildRaidOpen>
{
	public override void Run(long senderId, GuildRaidOpen protocolData)
	{
		string chat = LocalizationManager.GetString(GuildRaid.GUILDRAID_OPEN_MESSAGE, protocolData.Nick);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
	}
}
