using NGame2.NGuild;
using NGame2.NUI.NManager;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildBuildingInvestProtocolHandler : MessageProtocolHandler<GuildBuildingInvest>
{
	public override void Run(long senderId, GuildBuildingInvest protocolData)
	{
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			UserGuildManager myGuildManager = guildManager.MyGuildManager;
			if (myGuildManager != null)
			{
				myGuildManager.SetBuildingInvest(protocolData.GuildBuildingIndex, protocolData.InvestWood, protocolData.InvestStone, protocolData.InvestMetal);
				myGuildManager.SetConstructionResource(protocolData.Wood, protocolData.Stone, protocolData.Metal);
				SceneAutoBehaviour<GuildManagement2>.instance.RefreshGuildConstructionWindow();
			}
		}
	}
}
