using System;
using System.Collections.Generic;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
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

public class BaseWaitForNext<T> : StateBase where T : StateBase
{
	protected interface IJob
	{
		bool Complete(T This);
	}

	protected class CreateSceneChange : IJob
	{
		private bool _isLoadStart;

		private string _effectCode = "UI.Loading.KaselRun";

		public CreateSceneChange(string effectCode = "")
		{
			if (!String_Extension_Method.IsNullOrEmpty(effectCode))
			{
				_effectCode = effectCode;
			}
		}

		bool IJob.Complete(T This)
		{
			if (!(This is BaseWaitForNext<T> baseWaitForNext))
			{
				return true;
			}
			if (!_isLoadStart)
			{
				_isLoadStart = true;
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Freeze();
				if (GlobalTopmostWindow.IsValidInstance)
				{
					SoundStock stock = new SoundStock
					{
						Code = "UI.Eclipse.BattleFadein"
					};
					SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
					baseWaitForNext._sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.EclipseNextTeam);
				}
				ScreenEffect.StockData stockData = new ScreenEffect.StockData();
				stockData.Code = _effectCode;
				baseWaitForNext._effectContents = ScreenEffect.PlayEffect(GlobalUIRoot.ELayerType.GlobalUI, stockData);
			}
			int num;
			if (baseWaitForNext._sceneChanger != null)
			{
				num = (baseWaitForNext._sceneChanger.IsCompleteAnimation() ? 1 : 0);
				if (num != 0)
				{
					baseWaitForNext._sceneChanger.Pause();
					BattleViewGenericSingleton<CreatureManager>.instance.ResetTeamViewPosition(TeamType.Right);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}
	}

	protected class CloseSceneChange : IJob
	{
		private bool _isClose;

		bool IJob.Complete(T This)
		{
			if (!(This is BaseWaitForNext<T> baseWaitForNext))
			{
				return true;
			}
			if (!_isClose)
			{
				_isClose = true;
				SceneAutoBehaviour<BattleManagement>.instance.OpenMenu(isFirstOpen: false);
				SceneAutoBehaviour<BattleManagement>.instance.ReleaseMenuHide(BattleManagement.EMenuHideFlag.LastKill, force: true);
				if (baseWaitForNext._sceneChanger != null)
				{
					baseWaitForNext._sceneChanger.Resume();
					baseWaitForNext._sceneChanger.Close();
				}
				if (baseWaitForNext._effectContents != null)
				{
					baseWaitForNext._effectContents.ReleaseAll();
				}
				SoundStock stock = new SoundStock
				{
					Code = "UI.Eclipse.BattleFadeout"
				};
				SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
				SingletonUpdater<GCManager>.instance.CollectAll(delegate
				{
					SingletonUpdater<GCManager>.instance.CollectAll();
				});
			}
			if (baseWaitForNext._sceneChanger != null && !baseWaitForNext._sceneChanger.IsClosed())
			{
				return false;
			}
			return true;
		}
	}

	protected class WaitWaveMove : IJob
	{
		bool IJob.Complete(T This)
		{
			if (!(This is BaseWaitForNext<T> baseWaitForNext))
			{
				return true;
			}
			if (baseWaitForNext._handlerListWaveReady == null && !SceneAutoBehaviour<WavePositionCollector>.instance.IsContain(NShared.SetPosition.SetPositionMethodType.WaveReady))
			{
				return false;
			}
			if (baseWaitForNext._handlerListWaveReady == null)
			{
				baseWaitForNext._handlerListWaveReady = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_WaveReady();
				SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.WaveReady);
				if (!baseWaitForNext.IsIgnoreSwitchCamera(SwitchCameraType.WaveMove))
				{
					SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveMove(baseWaitForNext._handlerListWaveReady);
				}
			}
			if (baseWaitForNext._handlerListWaveReady != null)
			{
				for (int i = 0; i < baseWaitForNext._handlerListWaveReady.Count; i++)
				{
					if (baseWaitForNext._handlerListWaveReady[i].RemainedTime > 0.5f)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	protected class WaveReady : IJob
	{
		private bool _isComplete;

		bool IJob.Complete(T This)
		{
			if (_isComplete)
			{
				return true;
			}
			if (!This.IListener_.IsWaveReady(reset: true))
			{
				return false;
			}
			_isComplete = true;
			return true;
		}
	}

	protected class SendWaveResume : IJob
	{
		private bool _isSend;

		private bool _isComplete;

		bool IJob.Complete(T This)
		{
			if (!_isSend)
			{
				_isSend = true;
				This.IListener_.OnWaveResume(delegate
				{
					_isComplete = true;
				});
			}
			return _isComplete;
		}
	}

	protected class WaveMove : IJob
	{
		private bool _isSpawn;

		bool IJob.Complete(T This)
		{
			if (!(This is BaseWaitForNext<T> baseWaitForNext))
			{
				return true;
			}
			if (baseWaitForNext._handlerListWaveReady == null && !SceneAutoBehaviour<WavePositionCollector>.instance.IsContain(NShared.SetPosition.SetPositionMethodType.WaveReady))
			{
				CreateHandlerWaveReady(This);
				return false;
			}
			if (!_isSpawn)
			{
				_isSpawn = true;
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Opening();
				return false;
			}
			return true;
		}

		private void CreateHandlerWaveReady(T This)
		{
			if (This is BaseWaitForNext<T> { _handlerListWaveReady: null } baseWaitForNext)
			{
				baseWaitForNext._handlerListWaveReady = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_WaveReady();
				SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.WaveReady);
				if (!baseWaitForNext.IsIgnoreSwitchCamera(SwitchCameraType.WaveMove))
				{
					SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveMove(baseWaitForNext._handlerListWaveReady);
				}
			}
		}
	}

	protected class RunToForward : IJob
	{
		private bool _isStart = true;

		private DateTime _beginTime;

		private const int WaitSec = 1;

		bool IJob.Complete(T This)
		{
			if (!(This is BaseWaitForNext<T> baseWaitForNext))
			{
				return true;
			}
			if (_isStart)
			{
				if (!SceneAutoBehaviour<WavePositionCollector>.instance.IsContain(NShared.SetPosition.SetPositionMethodType.DungeonChangeMove))
				{
					return false;
				}
				_isStart = false;
				_beginTime = DateTime.Now;
				SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Freeze();
				baseWaitForNext._handlerListDungeonChangeMove = SceneAutoBehaviour<WavePositionCollector>.instance.CreateHandler_DungeonChangeMove();
				SceneAutoBehaviour<WavePositionCollector>.instance.Clear(NShared.SetPosition.SetPositionMethodType.DungeonChangeMove);
			}
			if ((DateTime.Now - _beginTime).TotalSeconds < 1.0)
			{
				return false;
			}
			return true;
		}
	}

	protected List<IJob> _jobList = new List<IJob>();

	protected List<NGame2.NBattleView.NCreature.NHandler.WaveReady> _handlerListWaveReady;

	private SceneChanger _sceneChanger;

	private IContents _effectContents;

	private List<DungeonChangeMove> _handlerListDungeonChangeMove;

	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
	}

	protected void ReleaseActivatedEffect()
	{
		if (_sceneChanger != null)
		{
			_sceneChanger.Resume();
			_sceneChanger.Close();
		}
		if (_effectContents != null)
		{
			_effectContents.ReleaseAll();
		}
	}
}
