using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class BecomeGuildGhostMemberProtocolHandler : MessageProtocolHandler<BecomeGuildGhostMembers>
{
	public override void Run(long senderId, BecomeGuildGhostMembers protocolData)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			return;
		}
		bool flag = false;
		long[] ghostMemberIds = protocolData.GhostMemberIds;
		foreach (long num in ghostMemberIds)
		{
			GlobalAutoBehaviour<GuildManager>.instance.MemberManager.SetRank(num, EGuildMemberRank.GuildGhostMember);
			if (num == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
			{
				flag = true;
			}
		}
		if (flag)
		{
			SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageUI();
		}
		SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
	}
}
