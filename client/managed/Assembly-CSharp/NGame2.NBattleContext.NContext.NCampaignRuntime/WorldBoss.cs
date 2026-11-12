using NGame2.NBattleField;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetWorldBossHp;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NBattleContext.NContext.NCampaignRuntime;

public class WorldBoss : ICampaignRuntime
{
	private int _worldBossIndex;

	private float _lastGetWorldBossHpTime;

	private bool _worldBossKilled;

	public WorldBoss(int worldBossIndex)
	{
		_worldBossIndex = worldBossIndex;
	}

	void ICampaignRuntime.Update()
	{
		if (!SingletonAuto<BattleFacade>.instance.GetCampaignCache().IsBattleEnd && !(Time.realtimeSinceStartup - _lastGetWorldBossHpTime < 10f) && _worldBossIndex != 0 && !_worldBossKilled)
		{
			_lastGetWorldBossHpTime = Time.realtimeSinceStartup;
			RequestGetWorldBossHp();
		}
	}

	private void RequestGetWorldBossHp()
	{
		NShared.GetWorldBossHp.Request request = new NShared.GetWorldBossHp.Request();
		request.WorldBossIndex = _worldBossIndex;
		WebRequestFlag flag = WebRequestFlag.Ping;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, flag, delegate(NShared.GetWorldBossHp.Response response)
		{
			if (!_worldBossKilled && response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success && response.Hp <= 0)
			{
				Stop(withdrawBattle: true);
			}
		});
	}

	public void Stop(bool withdrawBattle)
	{
		_worldBossKilled = true;
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
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.NLocalization.WorldBoss.WORLDBOSS_KILLED_BY_OTHER);
			GlobalMessagePopup.OpenGlobal(title, message, delegate
			{
			}, retryButton: false);
		}
	}
}
