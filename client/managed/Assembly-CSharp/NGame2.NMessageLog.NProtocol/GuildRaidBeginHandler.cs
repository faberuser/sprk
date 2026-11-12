using NGame2.NChatting;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidBeginHandler : MessageProtocolHandler<GuildRaidBegin>
{
	public override void Run(long senderId, GuildRaidBegin protocolData)
	{
		string chat = LocalizationManager.GetString(GuildRaid.GUILDRAID_BEGIN_MESSAGE, protocolData.Nick, protocolData.DungeonIndex);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
	}
}
