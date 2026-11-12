using NGame2.NGuild;
using NGame2.NUI.NManager;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildBuildingLevelUpProtocolHandler : MessageProtocolHandler<GuildBuildingLevelUp>
{
	public override void Run(long senderId, GuildBuildingLevelUp protocolData)
	{
		GuildManager guildManager = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance : null);
		if (!(guildManager == null))
		{
			UserGuildManager myGuildManager = guildManager.MyGuildManager;
			if (myGuildManager != null)
			{
				myGuildManager.SetBuildingLevel(protocolData.GuildBuildingIndex, protocolData.GuildBuildingLevel, protocolData.UpdatedTime);
				myGuildManager.SetConstructionResource(protocolData.Wood, protocolData.Stone, protocolData.Metal);
				myGuildManager.BuildingUpgradeApply(protocolData.GuildBuildingIndex, protocolData.GuildBuildingLevel);
				SceneAutoBehaviour<GuildManagement2>.instance.RefreshGuildConstructionWindow();
			}
		}
	}
}
