using NGame2.NChatting;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidBonusDungeonOpenHandler : MessageProtocolHandler<GuildRaidBonusDungeonOpen>
{
	public override void Run(long senderId, GuildRaidBonusDungeonOpen protocolData)
	{
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_RAID_BONUS_DUNGEON_OPEN", protocolData.Nick);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(stringDirect);
	}
}
