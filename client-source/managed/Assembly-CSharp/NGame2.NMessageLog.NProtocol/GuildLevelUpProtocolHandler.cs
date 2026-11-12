using NGame2.NGuild;
using NGame2.NUI.NManager;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildLevelUpProtocolHandler : MessageProtocolHandler<GuildLevelUp>
{
	public override void Run(long senderId, GuildLevelUp protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level = protocolData.Level;
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
	}
}
