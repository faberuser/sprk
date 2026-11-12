using System;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaitForNextTeam : BaseWaitForNext<WaitForNextTeam>
{
	private class WaitNextTeamPacket : IJob
	{
		bool IJob.Complete(WaitForNextTeam This)
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

		private const int WaitSec = 1;

		bool IJob.Complete(WaitForNextTeam This)
		{
			if (!_isLoadStart)
			{
				if (!AccountManager.IsLogined())
				{
					return true;
				}
				HeroInfo[] currentHeroInfos = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetCurrentHeroInfos();
				if (__Extension_Method_ICollection__.COUNT(currentHeroInfos) == 0)
				{
					return true;
				}
				_isLoadStart = true;
				_beginTime = DateTime.Now;
				preloader = new BattleCreaturePreLoader();
				preloader.EnterDungeon = This._listener.GetEnterDungeonData();
				HeroInfo[] array = currentHeroInfos;
				foreach (HeroInfo heroInfo in array)
				{
					preloader.AddCreature(heroInfo, null, loadCreatureImage: true);
				}
				preloader.Begin();
			}
			if (preloader != null && !preloader.IsEnd)
			{
				preloader.Tick();
				if (preloader.IsEnd)
				{
					((BaseScene.IPreload)preloader).OnEnd();
					BattleScene.AddCustomPreload("EclispeNextTeam", preloader);
					_isComplete = true;
				}
			}
			if (_isComplete)
			{
				return (DateTime.Now - _beginTime).TotalSeconds > 1.0;
			}
			return false;
		}
	}

	private new class WaitWaveMove : IJob
	{
		bool IJob.Complete(WaitForNextTeam This)
		{
			if (This._handlerListWaveReady != null)
			{
				for (int i = 0; i < This._handlerListWaveReady.Count; i++)
				{
					if (This._handlerListWaveReady[i].RemainedTime > 0.5f)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private new class WaveReady : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(WaitForNextTeam This)
		{
			if (_isComplete)
			{
				return true;
			}
			if (!This._listener.IsWaveReady(reset: true))
			{
				return false;
			}
			_isComplete = true;
			return true;
		}
	}

	private string EffectCodeKaselRun => "UI.EclipsDungeon.KaselRun";

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
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
		if (_listener.IsBattleEnd())
		{
			ReleaseActivatedEffect();
			Next(new CampaignEnding());
			return;
		}
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
		{
			if (!_jobList[0].Complete(this))
			{
				return;
			}
			DebugLog($"[4-2.WaitForNextTeam] Remove Job: {_jobList[0].GetType().Name}");
			_jobList.RemoveAt(0);
		}
		Next(new WaveOpening(_handlerListWaveReady));
	}
}
