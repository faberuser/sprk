using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class KickedGuildMemberProtocolHandler : MessageProtocolHandler<KickedGuildMember>
{
	public override void Run(long senderId, KickedGuildMember protocolData)
	{
		if (protocolData.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id)
		{
			GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(null);
			GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(null);
			GlobalAutoBehaviour<GuildManager>.instance.InitGuildJoinRequestPlayers(null);
			GlobalAutoBehaviour<GuildManager>.instance.RemoveAllGuildRaidBootyItems();
			GlobalAutoBehaviour<GuildManager>.instance.RemoveGuildAdventureNotice();
			LobbyScene2.ListReturnState.Clear();
			SceneAutoBehaviour<GuildManagement2>.instance.CloseAllActiveWindow();
			if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawTime = protocolData.GuildWithdrawTime;
			}
			PortalRenewal instance = SceneAutoBehaviour<PortalManagement>.instance.GetInstance<PortalRenewal>(openedOnly: true);
			if (instance != null)
			{
				instance.Close();
			}
			if (UserCampaignManager.ChapterIndex != 995)
			{
				return;
			}
			if (SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<EntryMenu>().IsOpen())
			{
				if (SceneAutoBehaviour<GuildManagement2>.instance.IsOpenedAny())
				{
					SceneAutoBehaviour<GuildManagement2>.instance.CloseAllActiveWindow();
				}
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseAllActiveWindow(immediately: true);
				SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
			}
			if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
			{
				GlobalAutoBehaviour<GuildManager>.instance.ExistGuildChpater();
			}
		}
		else
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Remove(protocolData.AccountId);
				SceneAutoBehaviour<GuildManagement2>.instance.UpdateGuildManageList(GuildManage.ListType.GuildMember);
			}
			string chat = LocalizationManager.GetString(Guild.GUILD_CHAT_KICKED, protocolData.Nick);
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
		}
	}
}
