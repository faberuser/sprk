using NGame2.NChatting;
using NGame2.NGuild;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SetGuildNameProtocolHandler : MessageProtocolHandler<SetGuildName>
{
	public override void Run(long senderId, SetGuildName protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildName(protocolData.Name);
		string chat = LocalizationManager.GetString(Item.USE_GUILD_NAME_CHANGE_MSG, protocolData.Name);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
	}
}
