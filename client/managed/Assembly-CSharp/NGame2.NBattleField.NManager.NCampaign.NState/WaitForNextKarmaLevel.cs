using System;
using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NGame3DWidget;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class WaitForNextKarmaLevel : StateBase
{
	private interface IJob
	{
		bool Complete(WaitForNextKarmaLevel This);
	}

	private class WaitNextLevelSetting : IJob
	{
		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (!This._listener.IsSetNextDungeon(reset: true))
			{
				return false;
			}
			return true;
		}
	}

	private class RunToBackWard : IJob
	{
		private bool _isStart = true;

		private bool _isCameraFreeze;

		private DateTime _beginTime;

		private const int WaitSec = 1;

		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (_isStart)
			{
				if (!_isCameraFreeze)
				{
					_isCameraFreeze = true;
					SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Freeze();
				}
				if (!SceneAutoBehaviour<WavePositionCollector>.instance.IsContain(NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward))
				{
					return false;
				}
				_isStart = false;
				_beginTime = DateTime.Now;
				This._handlerListDungeonChangeMove = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_DungeonChangeMoveBackward();
				SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.DungeonChangeMoveBackward);
			}
			if ((DateTime.Now - _beginTime).TotalSeconds < 1.0)
			{
				return false;
			}
			return true;
		}
	}

	private class CreateSceneChange : IJob
	{
		private bool _isLoadStart;

		private DateTime _beginTime;

		private const int WaitSec = 1;

		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (!_isLoadStart)
			{
				_isLoadStart = true;
				_beginTime = DateTime.Now;
				if (GlobalTopmostWindow.IsValidInstance)
				{
					SoundStock stock = new SoundStock
					{
						Code = "UI.Eclipse.BattleFadein"
					};
					SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
					This._sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.EclipseNextCampaign, 0.8f);
				}
				ScreenEffect.StockData stockData = new ScreenEffect.StockData();
				stockData.Code = "UI.EclipsDungeon.KaselRun";
				This._effectContents = ScreenEffect.PlayEffect(GlobalUIRoot.ELayerType.GlobalUI, stockData);
			}
			int num;
			if (This._sceneChanger != null)
			{
				num = (This._sceneChanger.IsCompleteAnimation() ? 1 : 0);
				if (num != 0)
				{
					This._sceneChanger.Pause();
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}
	}

	private class PreLoadDungeonData : IJob
	{
		private bool _isLoadStart;

		private bool _isComplete;

		private BattleCreaturePreLoader preloader;

		private DateTime _beginTime;

		private const int WaitSec = 2;

		bool IJob.Complete(WaitForNextKarmaLevel This)
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
					((BaseScene.IPreload)preloader).OnEnd();
					BattleScene.AddCustomPreload("EclispeNextDungeon", preloader);
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

	private class CloseSceneChange : IJob
	{
		private bool _isGCEnded;

		private bool _isClose;

		private bool _isStartGc;

		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (!_isStartGc)
			{
				SingletonUpdater<GCManager>.instance.CollectAll(delegate
				{
					_isGCEnded = true;
				});
				_isStartGc = true;
			}
			if (!_isGCEnded)
			{
				return false;
			}
			if (!_isClose)
			{
				_isClose = true;
				SceneAutoBehaviour<BattleManagement>.instance.OpenMenu(isFirstOpen: false);
				SceneAutoBehaviour<BattleManagement>.instance.ReleaseMenuHide(BattleManagement.EMenuHideFlag.LastKill, force: true);
				if (This._sceneChanger != null)
				{
					This._sceneChanger.Resume();
					This._sceneChanger.Close();
				}
				if (This._effectContents != null)
				{
					This._effectContents.ReleaseAll();
				}
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Opening();
				SoundStock stock = new SoundStock
				{
					Code = "UI.Eclipse.BattleFadeout"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
			}
			if (This._sceneChanger != null && !This._sceneChanger.IsClosed())
			{
				return false;
			}
			return true;
		}
	}

	private class WaitCardPick : IJob
	{
		private bool _isComplete;

		private bool _selectStage;

		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (!_selectStage)
			{
				This._karmaWindow.StartAnimation();
				_selectStage = true;
			}
			if (!This._karmaWindow.IsCardPickEnded())
			{
				return false;
			}
			return true;
		}
	}

	private class SendWaveResume : IJob
	{
		private bool _isSend;

		private bool _isComplete;

		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (!_isSend)
			{
				_isSend = true;
				This._listener.OnWaveResume(delegate
				{
					_isComplete = true;
				});
			}
			return _isComplete;
		}
	}

	private class WaveMove : IJob
	{
		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			This._handlerListWaveReady = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_WaveReady(includeRightTeam: true);
			SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.WaveReady);
			if (!This.IsIgnoreSwitchCamera(SwitchCameraType.WaveMove))
			{
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveMove(This._handlerListWaveReady);
			}
			if (SceneAutoBehaviour<BattleManagement>.instance.IsMenuHidden)
			{
				SceneAutoBehaviour<BattleManagement>.instance.ReleaseMenuHide(BattleManagement.EMenuHideFlag.LastKill, force: true);
			}
			return true;
		}
	}

	private class WaitWaveMove : IJob
	{
		bool IJob.Complete(WaitForNextKarmaLevel This)
		{
			if (This._handlerListWaveReady != null)
			{
				for (int i = 0; i < This._handlerListWaveReady.Count; i++)
				{
					if (This._handlerListWaveReady[i].RemainedTime > 0f)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private class WaveReady : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(WaitForNextKarmaLevel This)
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

	private const string EclipseKaselRunEffectCode = "UI.EclipsDungeon.KaselRun";

	private List<DungeonChangeMoveBackward> _handlerListDungeonChangeMove;

	private List<NGame2.NBattleView.NCreature.NHandler.WaveReady> _handlerListWaveReady;

	private SceneChanger _sceneChanger;

	private KarmaCardSelectWindow _karmaWindow;

	private IContents _effectContents;

	private List<IJob> _jobList = new List<IJob>();

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		_jobList.Add(new WaitNextLevelSetting());
		_jobList.Add(new WaveMove());
		_jobList.Add(new WaitWaveMove());
		_jobList.Add(new WaitCardPick());
		_jobList.Add(new SendWaveResume());
		_jobList.Add(new WaveReady());
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			_karmaWindow = SceneAutoBehaviour<BattleManagement>.instance.GetInstance<KarmaCardSelectWindow>();
		}
	}

	public override void ChainUpdate()
	{
		if (_listener.IsBattleEnd())
		{
			Next(new CampaignEnding());
			return;
		}
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(_jobList))
		{
			if (!_jobList[0].Complete(this))
			{
				return;
			}
			DebugLog($"[4-1.WaitForNextKarmaLevel] Remove Job: {_jobList[0].GetType().Name}");
			_jobList.RemoveAt(0);
		}
		Next(new WaveOpening(_handlerListWaveReady));
	}
}
