using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class JoinGuildRequestAcceptedProtocolHandler : MessageProtocolHandler<JoinGuildRequestAccepted>
{
	public override void Run(long senderId, JoinGuildRequestAccepted protocolData)
	{
		GlobalAutoBehaviour<GuildManager>.instance.SetJoinRequestGuilds(null);
		SceneAutoBehaviour<GuildManagement2>.instance.GetGuildBasicInfo(delegate
		{
			SceneAutoBehaviour<GuildManagement2>.instance.GetAllGuildMemberInfo(null);
		});
		SceneAutoBehaviour<GuildManagement2>.instance.CloseAllActiveWindow();
		if (SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<NGame2.NUI.NWindow.EntryMenu>().IsOpen())
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data != null && data.EntryNPCCode != null && string.Compare(data.EntryNPCCode, "GuildOffice") == 0)
			{
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
			}
		}
		string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_NEW_MEMBER, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick);
		GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
	}
}
