using System;
using NGame2.NBattleField;
using NGame2.NScene;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NShared.PingGuildRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NBattleContext.NContext.NCampaignRuntime;

public class GuildRaid : ICampaignRuntime
{
	private float _lastGuildRaidPingTime;

	void ICampaignRuntime.Update()
	{
		if (!SingletonAuto<BattleFacade>.instance.GetCampaignCache().IsBattleEnd && !(Time.realtimeSinceStartup - _lastGuildRaidPingTime < 20f))
		{
			_lastGuildRaidPingTime = Time.realtimeSinceStartup;
			RequestGuildRaidPing();
		}
	}

	private void RequestGuildRaidPing()
	{
		NShared.PingGuildRaid.Request request = new NShared.PingGuildRaid.Request();
		WebRequestFlag flag = WebRequestFlag.Ping;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.PingGuildRaid.Response response)
		{
			if (response == null)
			{
				OnGuildRaidPingFail(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				OnGuildRaidPingFail(response.BaseResult);
			}
			else if (response.Result != ResultType.Success)
			{
				OnGuildRaidPingFail(response.Result);
			}
		});
	}

	private void OnGuildRaidPingFail(Enum error)
	{
		MessagePopup.OpenError(error, delegate
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		});
	}

	public void Stop(bool withdrawBattle)
	{
		if (withdrawBattle)
		{
			SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
			SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
			SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
			SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				WaveWithdrawBattleLog runtime = new WaveWithdrawBattleLog();
				battleInstance.BattleLogContainer.AddLog(runtime);
			});
		}
	}
}
