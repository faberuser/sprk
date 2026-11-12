using NGame2.NBattleContext;
using NGame2.NBattleContext.NContext.NCampaignRuntime;
using NGame2.NChatting;
using NGame2.NGuild;
using NGame2.NScene;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildRaidEndHandler : MessageProtocolHandler<GuildRaidEnd>
{
	public override void Run(long senderId, GuildRaidEnd protocolData)
	{
		if (protocolData == null)
		{
			return;
		}
		GuildRaidDungeonInfo resultGuildRaidInfo = protocolData.ResultGuildRaidInfo;
		if (resultGuildRaidInfo != null)
		{
			UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
			if (myGuildManager != null && myGuildManager.GuildRaidManager != null)
			{
				myGuildManager.GuildRaidManager.Apply(resultGuildRaidInfo);
			}
		}
		GuildRaidChapterData data = GameRoot.Instance.GuildRaidChapterDataContainer.GetData(protocolData.ChapterIndex);
		if (data != null)
		{
			string chat = LocalizationManager.GetString(NShared.NLocalization.GuildRaid.GUILDRAID_END_MESSAGE, protocolData.Nick, protocolData.DungeonIndex);
			if (protocolData.Completed)
			{
				chat = (data.IsBonusDungeon ? LocalizationManager.GetStringDirect("GUILD_RAID_BONUS_DUNGEON_END", protocolData.Nick) : ((!data.IsDummyDungeon) ? LocalizationManager.GetString(NShared.NLocalization.GuildRaid.GUILDRAID_DUNGEON_CLEAR_MESSAGE, protocolData.Nick, protocolData.DungeonIndex) : LocalizationManager.GetString(NShared.NLocalization.GuildRaid.GUILDRAID_END_MESSAGE, protocolData.Nick, protocolData.DungeonIndex)));
			}
			GlobalAutoBehaviour<ChattingManager>.instance.AddGuildSystemChat(chat);
			StopCurrentGuildRaidBattle(protocolData);
		}
	}

	private void StopCurrentGuildRaidBattle(GuildRaidEnd protocolData)
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2 && SingletonAuto<CampaignContext>.isValidInstance && protocolData != null)
		{
			CampaignContext instance = SingletonAuto<CampaignContext>.instance;
			if (instance.IsGuildRaidCampaign() && instance.EnterDungeon != null && instance.EnterDungeon.Chapter == protocolData.ChapterIndex && instance.EnterDungeon.OriginalDungeon == protocolData.DungeonIndex && instance.GetCampaignRuntime() is NGame2.NBattleContext.NContext.NCampaignRuntime.GuildRaid guildRaid)
			{
				guildRaid.Stop(protocolData.Completed);
			}
		}
	}
}
