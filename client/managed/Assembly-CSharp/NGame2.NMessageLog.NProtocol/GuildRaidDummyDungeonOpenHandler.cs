using NGame2.NChatting;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidDummyDungeonOpenHandler : MessageProtocolHandler<GuildRaidDummyDungeonOpen>
{
	public override void Run(long senderId, GuildRaidDummyDungeonOpen protocolData)
	{
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_RAID_DUMMY_DUNGEON_OPEN", protocolData.Nick);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(stringDirect);
	}
}
