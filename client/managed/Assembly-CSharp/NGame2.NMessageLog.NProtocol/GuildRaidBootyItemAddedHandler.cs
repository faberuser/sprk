using NGame2.NGuild;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidBootyItemAddedHandler : MessageProtocolHandler<GuildRaidBootyItemAdded>
{
	public override void Run(long senderId, GuildRaidBootyItemAdded protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.ApplyAddedBootyItem(protocolData.EquipItemInfos, protocolData.ItemInfos);
	}
}
