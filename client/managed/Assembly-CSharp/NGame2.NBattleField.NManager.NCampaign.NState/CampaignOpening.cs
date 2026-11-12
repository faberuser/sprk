using System;
using System.Collections.Generic;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NFunctionality;
using NGame2.NScene;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class CampaignOpening : StateBase
{
	private interface IJob
	{
		bool Complete(CampaignOpening This);
	}

	private class WaveReady : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(CampaignOpening This)
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

	private class EnterBattleEvent : IJob
	{
		private bool _isRaiseEvent;

		private bool _isComplete;

		bool IJob.Complete(CampaignOpening This)
		{
			if (!_isRaiseEvent)
			{
				int chapterIndex = This._listener.GetChapterIndex();
				int dungeonIndex = This._listener.GetDungeonIndex();
				IEvent triggerEvent = ((!This._listener.IsScenarioDungeon()) ? ((BaseNodeEvent)new EnterBattle
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex
				}) : ((BaseNodeEvent)new EnterScenarioBattle
				{
					ChapterIndex = chapterIndex,
					DungeonIndex = dungeonIndex
				}));
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
				{
					_isComplete = true;
				}, delegate
				{
					Vespa_System_Action_Extension_Method.fire(This._onCampaignReady);
				});
				_isRaiseEvent = true;
			}
			return _isComplete;
		}
	}

	private class WaitForLoadingEnd : IJob
	{
		bool IJob.Complete(CampaignOpening This)
		{
			return !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading;
		}
	}

	private class DisplayOpening : IJob
	{
		private bool _begin;

		private bool _isComplete;

		bool IJob.Complete(CampaignOpening This)
		{
			if (!_begin)
			{
				This._listener.OnCampaignBegin();
				This._listener.OnCampaignReady();
				PlayBattleStartEffect(This);
				_begin = true;
			}
			return _isComplete;
		}

		private void PlayBattleStartEffect(CampaignOpening This)
		{
			if (GlobalAutoBehaviour<FunctionalityManager>.instance.Check(FunctionalityType.SkipBattleStartEffect, value: true))
			{
				_isComplete = true;
				return;
			}
			BattleStartEffectData battleStartEffectData = This._listener.GetBattleStartEffectData();
			string text = ((battleStartEffectData != null) ? battleStartEffectData.ScreenEffectCode : "UI.Battle.BattleStart");
			string text2 = ((battleStartEffectData != null) ? battleStartEffectData.SoundEffectCode : "UI.Battle.BattleStart");
			if (!String_Extension_Method.IsNullOrEmpty(text2))
			{
				SoundStock stock = new SoundStock
				{
					Code = text2,
					LocalTimer = GlobalTimer.DefaultLocal
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
			}
			if (battleStartEffectData != null && !String_Extension_Method.IsNullOrEmpty(battleStartEffectData.PrefabWindow))
			{
				new WindowPrefab<StartEffectWindow>(battleStartEffectData.PrefabWindow, GameUIRoot.ELayerType.UI).instance.OpenEffect(delegate
				{
					_isComplete = true;
				});
			}
			else if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				ScreenEffect.PlayEffect(text, null, delegate
				{
					_isComplete = true;
				});
			}
			else
			{
				_isComplete = true;
			}
		}
	}

	private Action _onCampaignReady;

	private List<IJob> _jobList = new List<IJob>();

	public CampaignOpening(Action onCampaignReady)
	{
		_onCampaignReady = onCampaignReady;
	}

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		Game3DRoot.instance.GameCamera.View = SceneAutoBehaviour<CameraManager2>.instance;
		SceneAutoBehaviour<WavePositionCollector>.instance.BeginCollect();
		_jobList.Add(new WaveReady());
		_jobList.Add(new EnterBattleEvent());
		_jobList.Add(new WaitForLoadingEnd());
		_jobList.Add(new DisplayOpening());
	}

	public override void ChainUpdate()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
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
