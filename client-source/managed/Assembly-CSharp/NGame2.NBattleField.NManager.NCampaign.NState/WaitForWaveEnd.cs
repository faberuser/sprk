using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager.NBattle;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaitForWaveEnd : StateBase
{
	private int _currentWaveIndex;

	private bool _waveStartRaised;

	private bool _isBossStatusVisible;

	private bool _isLastWave;

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_currentWaveIndex = _listener.GetCurrentWaveIndex();
		_waveStartRaised = false;
		_isBossStatusVisible = false;
		_isLastWave = false;
		SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
		SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
	}

	public override void OnEnd()
	{
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		if (!_waveStartRaised)
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (battleCache != null && battleCache.IsBattleEnabled)
			{
				_waveStartRaised = true;
				int chapterIndex = _listener.GetChapterIndex();
				int dungeonIndex = _listener.GetDungeonIndex();
				_isLastWave = _currentWaveIndex == battleCache.TotalWaveIndex;
				IEvent triggerEvent = ((!_listener.IsScenarioDungeon()) ? ((BaseWaveEvent)new WaveStart
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = _currentWaveIndex
				}) : ((BaseWaveEvent)new ScenarioWaveStart
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = _currentWaveIndex
				}));
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
				if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
				{
					BattleViewGenericSingleton<FieldMapManager2>.instance.OnEvent(EBattleFieldEventType.WaveStart, _currentWaveIndex);
				}
			}
		}
		UpdateBossStatus();
		if (BattleViewGenericSingleton<CreatureManager>.instance.LastKillAnimating || BattleViewGenericSingleton<CreatureManager>.instance.IsDying() || !_listener.IsWaveEnd(_currentWaveIndex, reset: true))
		{
			return;
		}
		SceneAutoBehaviour<WavePositionCollector>.instance.BeginCollect();
		BattleFacade.BattleCacheData battleCache2 = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache2 != null)
		{
			switch (battleCache2.BattleType)
			{
			case BattleType.PunishmentRaid:
				if (_listener.WaveWinner() == TeamType.Left && !_isLastWave)
				{
					Next(new WaitForGimmick());
					return;
				}
				break;
			case BattleType.KarmaRaid:
				if (_listener.WaveWinner() == TeamType.Left)
				{
					if (_isLastWave)
					{
						SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
						Next(new WaitForNextKarmaLevel());
						return;
					}
					break;
				}
				SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
				Next(new WaveEnding());
				return;
			case BattleType.ArcdimRaid:
			case BattleType.ArcdimHardRaid:
				if (_listener.WaveWinner() == TeamType.Left && !_isLastWave)
				{
					SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
					Next(new WaitForForceNext());
					return;
				}
				break;
			case BattleType.Eclipse:
				if (_listener.WaveWinner() == TeamType.Left)
				{
					if (_isLastWave)
					{
						SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
						Next(new WaitForNextCampaign());
						return;
					}
					break;
				}
				SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.LastKill);
				Next(new WaveEnding());
				return;
			}
		}
		if (!IsIgnoreSwitchCamera(SwitchCameraType.WaveEnd))
		{
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveEnd(_currentWaveIndex);
		}
		Next(new WaveEnding());
	}

	private void UpdateBossStatus()
	{
		Creature bossCreature = StateBase.GetBossCreature(_currentWaveIndex);
		bool flag = bossCreature != null;
		if (flag != _isBossStatusVisible)
		{
			_isBossStatusVisible = flag;
			if (flag)
			{
				SceneAutoBehaviour<BattleManagement>.instance.OpenBossStatus(bossCreature);
				SceneAutoBehaviour<BattleManagement>.instance.OpenPatternAlarm(bossCreature);
			}
			else
			{
				SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
				SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
				SceneAutoBehaviour<BattleManagement>.instance.CloseShardGuageWindow();
			}
		}
	}
}
