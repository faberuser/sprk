using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class SetUserNickProtocolHandler : MessageProtocolHandler<SetUserNick>
{
	public override void Run(long senderId, SetUserNick protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.SetGuildMemberNick(protocolData.AccountId, protocolData.Nick);
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
		GlobalAutoBehaviour<AccountManager>.instance.FriendManager.SetNick(protocolData.AccountId, protocolData.Nick);
	}
}
