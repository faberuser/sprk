using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class DelegateGuildMasterProtocolHandler : MessageProtocolHandler<DelegateGuildMaster>
{
	public override void Run(long senderId, DelegateGuildMaster protocolData)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null || GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMasterMemberInfo().AccountId == protocolData.CurMasterId)
		{
			return;
		}
		bool num = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMasterMemberInfo().AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		bool flag = protocolData.CurMasterId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		if (!num)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SwapMasterMemberInfo(protocolData.CurMasterId);
			if (flag)
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			}
			else
			{
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			}
			string text = LocalizationManager.GetString(Guild.GUILD_RANK_MASTER);
			string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_CHANGE_RANK, protocolData.CurMasterNick, text);
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
		}
	}
}
