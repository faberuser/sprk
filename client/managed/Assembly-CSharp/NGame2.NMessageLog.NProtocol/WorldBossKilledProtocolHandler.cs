using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleContext.NContext.NCampaignRuntime;
using NGame2.NChatting;
using NGame2.NContentsInfo;
using NGame2.NScene;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class WorldBossKilledProtocolHandler : MessageProtocolHandler<WorldBossKilled>
{
	public override void Run(long senderId, WorldBossKilled protocolData)
	{
		if (protocolData != null)
		{
			SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.ClearLastWorldBossInfo();
			string chat = LocalizationManager.GetString(NShared.NLocalization.WorldBoss.WORLDBOSS_KILLED_BY_USER, protocolData.KillUserName);
			GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(chat, ChatType.Channel, showPreview: true, 0L);
			StopCurrentWorldBossBattle(protocolData);
		}
	}

	private void StopCurrentWorldBossBattle(WorldBossKilled protocolData)
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2 && SingletonAuto<CampaignContext>.isValidInstance)
		{
			CampaignContext instance = SingletonAuto<CampaignContext>.instance;
			if (instance.IsWorldBossCampaign() && instance.WorldBossIndex == protocolData.WorldBossIndex && instance.GetCampaignRuntime() is NGame2.NBattleContext.NContext.NCampaignRuntime.WorldBoss worldBoss)
			{
				bool withdrawBattle = protocolData.KillAccountId != GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
				worldBoss.Stop(withdrawBattle);
			}
		}
	}
}
