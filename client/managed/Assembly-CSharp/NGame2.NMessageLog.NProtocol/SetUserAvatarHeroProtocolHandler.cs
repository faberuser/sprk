using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SetUserAvatarHeroProtocolHandler : MessageProtocolHandler<SetUserAvatarHero>
{
	public override void Run(long senderId, SetUserAvatarHero protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.SetGuildMemberAvatarHero(protocolData.AccountId, protocolData.AvatarHeroIndex);
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
	}
}
