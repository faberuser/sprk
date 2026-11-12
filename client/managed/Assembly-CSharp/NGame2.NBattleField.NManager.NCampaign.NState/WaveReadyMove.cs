using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaveReadyMove : StateBase
{
	private interface IJob
	{
		bool Complete(WaveReadyMove This);
	}

	private class WaveMove : IJob
	{
		bool IJob.Complete(WaveReadyMove This)
		{
			This._handlerList = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_WaveReady();
			SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.WaveReady);
			if (!This.IsIgnoreSwitchCamera(SwitchCameraType.WaveMove))
			{
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveMove(This._handlerList);
			}
			if (SceneAutoBehaviour<BattleManagement>.instance.IsMenuHidden)
			{
				SceneAutoBehaviour<BattleManagement>.instance.ReleaseMenuHide(BattleManagement.EMenuHideFlag.LastKill, force: true);
			}
			return true;
		}
	}

	private class BossWaveEffect : IJob
	{
		bool IJob.Complete(WaveReadyMove This)
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			int chapterIndex = This._listener.GetChapterIndex();
			int dungeonIndex = This._listener.GetDungeonIndex();
			DungeonDifficultyType difficulty = This._listener.GetDifficulty();
			bool scenario = This._listener.IsScenarioDungeon();
			int waveIndex = battleCache.WaveIndex;
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, waveIndex);
			if (data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.BossList) && waveIndex > 1)
			{
				ScreenEffect.PlayEffect("UI.Battle.BossWave", null, null);
				ScreenEffect.PlaySound("UI.Boss.Warning", null);
			}
			return true;
		}
	}

	private class WaitWaveMove : IJob
	{
		bool IJob.Complete(WaveReadyMove This)
		{
			if (This._handlerList != null)
			{
				for (int i = 0; i < This._handlerList.Count; i++)
				{
					if (This._handlerList[i].RemainedTime > 0.5f)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private List<IJob> _jobList = new List<IJob>();

	private List<NGame2.NBattleView.NCreature.NHandler.WaveReady> _handlerList;

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_jobList.Add(new WaveMove());
		_jobList.Add(new BossWaveEffect());
		_jobList.Add(new WaitWaveMove());
	}

	public override void ChainUpdate()
	{
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
		{
			if (!_jobList[0].Complete(this))
			{
				return;
			}
			_jobList.RemoveAt(0);
		}
		Next(new WaveOpening(_handlerList));
	}
}
