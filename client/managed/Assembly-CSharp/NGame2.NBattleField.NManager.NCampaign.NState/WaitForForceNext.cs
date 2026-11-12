using System;
using NGame2.NScene.NPreloader;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaitForForceNext : BaseWaitForNext<WaitForForceNext>
{
	private class WaitNextTeamPacket : IJob
	{
		bool IJob.Complete(WaitForForceNext This)
		{
			if (!This._listener.IsSetNextTeam(reset: true))
			{
				return false;
			}
			return true;
		}
	}

	private class PreLoadDungeonData : IJob
	{
		private bool _isLoadStart;

		private bool _isComplete;

		private BattleCreaturePreLoader preloader;

		private DateTime _beginTime;

		private const int WaitSec = 2;

		bool IJob.Complete(WaitForForceNext This)
		{
			if (!_isLoadStart)
			{
				_isLoadStart = true;
				_beginTime = DateTime.Now;
				preloader = new BattleCreaturePreLoader();
				preloader.EnterDungeon = This._listener.GetEnterDungeonData();
				preloader.Begin();
			}
			if (preloader != null && !preloader.IsEnd)
			{
				preloader.Tick();
				if (preloader.IsEnd)
				{
					_isComplete = true;
				}
			}
			if (_isComplete)
			{
				return (DateTime.Now - _beginTime).TotalSeconds > 2.0;
			}
			return false;
		}
	}

	private string EffectCodeKaselRun => "UI.Loading.KaselRun";

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_jobList.Clear();
		_jobList.Add(new CreateSceneChange(EffectCodeKaselRun));
		_jobList.Add(new WaitNextTeamPacket());
		_jobList.Add(new PreLoadDungeonData());
		_jobList.Add(new SendWaveResume());
		_jobList.Add(new WaveMove());
		_jobList.Add(new CloseSceneChange());
		_jobList.Add(new WaitWaveMove());
		_jobList.Add(new WaveReady());
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
		Next(new WaveOpening(_handlerListWaveReady));
	}
}
