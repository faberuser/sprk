using NGame2.NSocket;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NVespa.NSingleton;
using NVespa.NSocket;

namespace NGame2.NBattleContext.NContext.NCampaignRuntime;

public class GuildSuppress : ICampaignRuntime
{
	void ICampaignRuntime.Update()
	{
	}

	public void Stop(bool withdrawBattle)
	{
		if (withdrawBattle)
		{
			SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
			SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
			SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
			if (GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.State == ConnectState.Connected)
			{
				GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request<WithdrawBattleReq, WithdrawBattleRes>(new WithdrawBattleReq(), null);
			}
		}
	}
}
