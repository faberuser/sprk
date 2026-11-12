using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleContext.NContext.NCampaignRuntime;
using NGame2.NScene;
using NShared;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NMessageLog.NProtocol;

public class GuildSuppressKilledProtocolHandler : MessageProtocolHandler<GuildSuppressKilled>
{
	public override void Run(long senderId, GuildSuppressKilled protocolData)
	{
		if (protocolData != null)
		{
			StopCurrentGuildSuppressBattle(protocolData);
		}
	}

	private void StopCurrentGuildSuppressBattle(GuildSuppressKilled protocolData)
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Battle2 || !SingletonAuto<CampaignContext>.isValidInstance)
		{
			return;
		}
		CampaignContext instance = SingletonAuto<CampaignContext>.instance;
		if (!(instance.GetCampaignRuntime() is GuildSuppress guildSuppress))
		{
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(instance.RaidIndex, instance.RaidLevel);
		if (data != null)
		{
			int num = instance.RaidLevel;
			if (data.Type == RaidType.GuildSuppressSingle)
			{
				num = GameRoot.Instance.GuildSuppressSessionDataContainer.GetMultiLevel(instance.RaidLevel);
			}
			if (num == protocolData.Level)
			{
				bool withdrawBattle = !TypeIterator.Contains(protocolData.KillAccountIds, GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
				guildSuppress.Stop(withdrawBattle);
			}
		}
	}
}
