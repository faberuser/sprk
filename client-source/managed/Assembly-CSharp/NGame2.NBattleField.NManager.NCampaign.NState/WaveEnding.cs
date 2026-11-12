using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NFunctionality;
using NGame2.NUI.NManager.NBattle;
using NGameAsset;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaveEnding : StateBase
{
	private interface IJob
	{
		bool Complete(WaveEnding This);
	}

	private class WaveReady : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(WaveEnding This)
		{
			if (_isComplete)
			{
				return true;
			}
			if (!This._listener.IsWaveReady(reset: true))
			{
				return false;
			}
			BattleViewGenericSingleton<CreatureManager>.instance.ReleaseExternalHandler();
			SceneAutoBehaviour<BattleManagement>.instance.CloseBossStatus();
			SceneAutoBehaviour<BattleManagement>.instance.ClosePatternAlarm();
			_isComplete = true;
			return true;
		}
	}

	private class WaveEndEvent : IJob
	{
		private bool _isRaiseEvent;

		private bool _isComplete;

		bool IJob.Complete(WaveEnding This)
		{
			if (!_isRaiseEvent)
			{
				_isRaiseEvent = true;
				BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
				int chapterIndex = This._listener.GetChapterIndex();
				int dungeonIndex = This._listener.GetDungeonIndex();
				int num = battleCache.WaveIndex - 1;
				IEvent triggerEvent = ((!This._listener.IsScenarioDungeon()) ? ((BaseNodeEvent)new WaveEnd
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = num
				}) : ((BaseNodeEvent)new ScenarioWaveEnd
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = num
				}));
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
				{
					_isComplete = true;
				});
				if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
				{
					BattleViewGenericSingleton<FieldMapManager2>.instance.OnEvent(EBattleFieldEventType.WaveEnd, num);
				}
			}
			return _isComplete;
		}
	}

	private class FunctionalityWait : IJob
	{
		bool IJob.Complete(WaveEnding This)
		{
			return GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.WaitOnWaveEndEvent, value: false);
		}
	}

	private class CompleteDrop : IJob
	{
		bool IJob.Complete(WaveEnding This)
		{
			return BattleViewGenericSingleton<NGame2.NBattleView.DropItemManager>.instance.CompleteDrop;
		}
	}

	private class IPhoneLowMemCollectEvent : IJob
	{
		private bool _isRaiseEvent;

		private bool _isComplete;

		bool IJob.Complete(WaveEnding This)
		{
			if (!_isRaiseEvent)
			{
				_isRaiseEvent = true;
				SingletonUpdater<GCManager>.instance.IPhoneLowMemCollect();
			}
			return _isComplete;
		}
	}

	private List<IJob> _jobList = new List<IJob>();

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_jobList.Add(new WaveReady());
		_jobList.Add(new WaveEndEvent());
		_jobList.Add(new FunctionalityWait());
		_jobList.Add(new CompleteDrop());
	}

	public override void ChainUpdate()
	{
		if (_listener.IsBattleEnd())
		{
			Next(new CampaignEnding());
		}
		else if (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
		{
			if (_jobList[0].Complete(this))
			{
				_jobList.RemoveAt(0);
			}
		}
		else
		{
			Next(new WaveReadyMove());
		}
	}
}
