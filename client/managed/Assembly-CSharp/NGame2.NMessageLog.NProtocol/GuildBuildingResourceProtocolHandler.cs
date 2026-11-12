using NGame2.NGuild;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildBuildingResourceProtocolHandler : MessageProtocolHandler<GuildBuildingResource>
{
	public override void Run(long senderId, GuildBuildingResource protocolData)
	{
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			guildManager.MyGuildManager?.SetConstructionResource(protocolData.Wood, protocolData.Stone, protocolData.Metal);
		}
	}
}
