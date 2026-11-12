using System;
using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NEventTrigger;
using NGame2.NFunctionality;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared.NCampaign.NCampaignState;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaveOpening : StateBase
{
	private interface IJob
	{
		bool Complete(WaveOpening This);
	}

	private class WaveReadyEvent : IJob
	{
		private bool _isRaiseEvent;

		private bool _isComplete;

		bool IJob.Complete(WaveOpening This)
		{
			if (!_isRaiseEvent)
			{
				_isRaiseEvent = true;
				BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
				int chapterIndex = This._listener.GetChapterIndex();
				int dungeonIndex = This._listener.GetDungeonIndex();
				int waveIndex = battleCache.WaveIndex;
				IEvent triggerEvent = ((!This._listener.IsScenarioDungeon()) ? ((BaseWaveEvent)new NShared.NEvent.WaveReady
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = waveIndex
				}) : ((BaseWaveEvent)new ScenarioWaveReady
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex,
					WaveIndex = waveIndex
				}));
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
				{
					_isComplete = true;
				});
				if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
				{
					BattleViewGenericSingleton<FieldMapManager2>.instance.OnEvent(EBattleFieldEventType.WaveReady, waveIndex);
				}
			}
			return _isComplete;
		}
	}

	private class FunctionalityWait : IJob
	{
		bool IJob.Complete(WaveOpening This)
		{
			return GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.WaitOnWaveReadyEvent, value: false);
		}
	}

	private class PlayBossIntro : IJob
	{
		private class SkipBossIntroListener : BossIntroWindow.IListener
		{
			public Action SkipIntro;

			public void OnClickSkip()
			{
				Vespa_System_Action_Extension_Method.fire(SkipIntro);
			}
		}

		private bool _isPlay;

		private bool _isComplete;

		private List<List<IRuntime>> _bossIntroRuntimeContainer;

		private void SkipRuntimeBossIntro()
		{
			ICamera camera = null;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_bossIntroRuntimeContainer); i++)
			{
				List<IRuntime> list = _bossIntroRuntimeContainer[i];
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list); j++)
				{
					IRuntime runtime = list[j];
					if (!runtime.IsEnd())
					{
						if (camera == null)
						{
							camera = runtime.Variable.RUNTIMEINSTANCE().SCENECONTAINER().GETCAMERA();
						}
						runtime.RequestStop(ERequestStop.None);
					}
				}
			}
			if (camera != null)
			{
				AlphaToggleEntry alphaToggleEntry = new AlphaToggleEntry();
				alphaToggleEntry.AlphaCurve = new AnimationCurve();
				alphaToggleEntry.TimeLength = 0.1f;
				camera.SetAlphaToggleEntry(alphaToggleEntry);
			}
		}

		bool IJob.Complete(WaveOpening This)
		{
			if (WavePlaying.ForceDecideWinner)
			{
				_isComplete = true;
			}
			if (_isComplete)
			{
				return true;
			}
			if (!_isPlay)
			{
				_isPlay = true;
				bool canSkip = false;
				if (!SendBossIntroMessage(out canSkip))
				{
					_isComplete = true;
					return true;
				}
				if (canSkip)
				{
					OpenSkipWindow();
				}
			}
			bool flag = true;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_bossIntroRuntimeContainer); i++)
			{
				List<IRuntime> list = _bossIntroRuntimeContainer[i];
				int num;
				for (num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
				{
					IRuntime runtime = list[num];
					if (!runtime.IsEnd())
					{
						return false;
					}
					runtime.ReleaseSelf();
					list.RemoveAt(num);
					num--;
				}
			}
			if (!flag)
			{
				return false;
			}
			OnComplete();
			_isComplete = true;
			return true;
		}

		private void OpenSkipWindow()
		{
			if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
			{
				SceneAutoBehaviour<BattleManagement>.instance.GetInstance<BossIntroWindow>().Open(new SkipBossIntroListener
				{
					SkipIntro = SkipRuntimeBossIntro
				});
			}
		}

		private void OnComplete()
		{
			CloseSkipWindow();
		}

		private void CloseSkipWindow()
		{
			if (SceneAutoBehaviour<BattleManagement>.isValidInstance && SceneAutoBehaviour<BattleManagement>.instance.IsOpened<BossIntroWindow>())
			{
				SceneAutoBehaviour<BattleManagement>.instance.GetInstance<BossIntroWindow>().Close();
			}
		}

		private bool SendBossIntroMessage(out bool canSkip)
		{
			canSkip = false;
			List<Creature> bossList = StateBase.GetBossList(99);
			if (bossList == null)
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(bossList); i++)
			{
				if (bossList[i].VisualData != null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			_bossIntroRuntimeContainer = SetBossIntros(bossList, out canSkip);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_bossIntroRuntimeContainer))
			{
				return false;
			}
			int waveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle(waveIndex);
			return true;
		}

		private List<List<IRuntime>> SetBossIntros(List<Creature> bossCreatures, out bool canSkip)
		{
			canSkip = false;
			List<List<IRuntime>> list = new List<List<IRuntime>>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(bossCreatures); i++)
			{
				Creature creature = bossCreatures[i];
				if (creature.VisualData == null)
				{
					continue;
				}
				CreatureEventMessage creatureEventMessage = creature.CreateCreatureEventMessage(CreatureEventMessage.EEventType.BossIntro, creature.VisualData.OnBossIntro);
				if (creatureEventMessage != null)
				{
					if (creature.VisualData.CanSkipBossIntro)
					{
						canSkip = true;
					}
					SingletonAuto<GlobalManager>.instance.HandleDirect(creatureEventMessage);
					list.Add(creatureEventMessage.PopRuntimeList());
					Vespa_Common_Class_Extension_Method.Interface((IMessage)creatureEventMessage).ReleaseSelf();
				}
			}
			return list;
		}
	}

	private class WaveMoveWait : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(WaveOpening This)
		{
			if (_isComplete)
			{
				return true;
			}
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
			_isComplete = true;
			return _isComplete;
		}
	}

	private class StartBattle : IJob
	{
		private float _elapsed;

		private bool _isComplete;

		bool IJob.Complete(WaveOpening This)
		{
			if (_isComplete)
			{
				return true;
			}
			_elapsed += Time.deltaTime;
			if (_elapsed < DelayTime)
			{
				return false;
			}
			_isComplete = true;
			int waveIndex = SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
			SceneAutoBehaviour<WavePositionCollector>.instance.EndCollect();
			BattleViewGenericSingleton<CreatureManager>.instance.ReleaseExternalHandler();
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle(waveIndex);
			string currentWaveBgm = This._listener.GetCurrentWaveBgm();
			if (!String_Extension_Method.IsNullOrEmpty(currentWaveBgm))
			{
				SingletonAuto<BgmManager2>.instance.Request(currentWaveBgm, isEvent: false);
			}
			This._listener.StartBattle();
			return _isComplete;
		}
	}

	public static float DelayTime;

	private List<NGame2.NBattleView.NCreature.NHandler.WaveReady> _handlerList;

	private List<IJob> _jobList = new List<IJob>();

	public WaveOpening(List<NGame2.NBattleView.NCreature.NHandler.WaveReady> handlerList)
	{
		_handlerList = handlerList;
	}

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_jobList.Add(new WaveReadyEvent());
		_jobList.Add(new FunctionalityWait());
		_jobList.Add(new PlayBossIntro());
		_jobList.Add(new WaveMoveWait());
		_jobList.Add(new StartBattle());
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
		Next(new WaitForWaveEnd());
	}
}
