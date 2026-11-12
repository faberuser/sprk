using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NRender3D;
using NGame2.NUI.NRender3D.NRenderTargetCinema;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroFullSceneViewBase : FrameControlWindow, MotionSequence.IDataProvider
{
	public interface Listener
	{
		void OnOpen();

		void OnClose();
	}

	public enum TouchMode
	{
		None,
		Rotate,
		Scale
	}

	[Header("[HeroFullSceneViewBase]")]
	public UIWidget NormalMenuWidget;

	public UIButton NormalCloseButton;

	public UIButton PhotoCloseButton;

	public UITexture Hero3DTexture;

	public UILabel HeroTitleLabel;

	public UILabel HeroNameLabel;

	public GameObject PauseAnimObject;

	public GameObject ResumeAnimObject;

	public UILabel HeroDescLabel;

	public float RotateSensitivity = 1f;

	public float ScaleSensitivity = 1f;

	public Vector3 NormalModeExtraOffset = Vector3.zero;

	public Vector3 PhotoModeExtraOffset = Vector3.zero;

	protected Listener _listener;

	protected int _heroUid;

	protected RenderTargetCinema _cinema;

	protected MotionSequence _motionSequence;

	protected CameraController _cameraController;

	protected TouchMode _touchMode;

	protected List<UICamera.MouseOrTouch> _touchList = new List<UICamera.MouseOrTouch>();

	protected Vector2 _firstTouchPositionForTest;

	protected float _prevScaleAmount;

	protected bool _photoMode;

	private System.Action _onEnd;

	private VespaCoroutine.RuntimeData _runtimeAnimation;

	public CachedHeroInfo HeroInfo { get; private set; }

	public PetInfo PetInfo { get; private set; }

	public CreatureData CreatureData { get; private set; }

	public bool _sceneChangerFinished { get; protected set; }

	protected virtual string _nodeTreePackCode => "Common/Cinema";

	protected virtual string _nodeTreeName => "HeroDetailView";

	public CameraController CameraController => _cameraController;

	public void Open(CachedHeroInfo heroInfo, Listener listener, System.Action onEnd = null, bool useSceneChanger = true)
	{
		_onEnd = onEnd;
		new DelegateTask(OpenAsync(heroInfo, listener, useSceneChanger)).Commit();
	}

	public void Open(PetInfo petInfo, Listener listener, System.Action onEnd = null, bool useSceneChanger = true)
	{
		_onEnd = onEnd;
		new DelegateTask(OpenAsync(petInfo, listener, useSceneChanger)).Commit();
	}

	protected ITransformInstnace GetCustomizeItemInstance(CustomizeStock stock)
	{
		if (_heroUid == 0 || _cinema == null)
		{
			return null;
		}
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
		if (rENDERFACADE == null)
		{
			return null;
		}
		if (rENDERFACADE.AssetCreatureAsync.Value is CreatureInstance creatureInstance)
		{
			return Vespa_Common_Class_Extension_Method.Interface((ICreature)creatureInstance).GetCutomizeItemInstance(stock);
		}
		return null;
	}

	protected void ChangeRanderFacadeAccessory(AccessoryPartType partType, AccessoryCostumeData acceesoryCostumeData, PositionInfo positionInfo)
	{
		if (_heroUid == 0 || _cinema == null)
		{
			return;
		}
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
		if (rENDERFACADE == null)
		{
			return;
		}
		if (acceesoryCostumeData == null)
		{
			CustomizeStock costumeStock = CustomizeStock.CreateEmptyPart(partType);
			rENDERFACADE.AssetCreatureAsync.ChangeCustomizeStock(costumeStock);
			return;
		}
		CustomizeStock stock = default(CustomizeStock);
		if (RenderFacadeBuilder.TryAssignCustomizeStock(ref stock, HeroInfo.HeroIndex, acceesoryCostumeData, positionInfo))
		{
			rENDERFACADE.AssetCreatureAsync.ChangeCustomizeStock(stock);
		}
	}

	protected void ChangeRenderFacadeWeapon(string weaponPrefix, int weaponStar, bool isEquipSoulWeapon, int soulWeaponGrade)
	{
		if (_heroUid != 0 && !(_cinema == null))
		{
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (!(rENDERFACADE == null) && rENDERFACADE.AssetCreatureAsync != null)
			{
				rENDERFACADE.AssetCreatureAsync.ChangeWeaponStock(new WeaponStock(weaponPrefix, weaponStar, isEquipSoulWeapon, soulWeaponGrade));
			}
		}
	}

	protected void ChangeRenderFacadeHair(string hairPrefix)
	{
		if (_heroUid != 0 && !(_cinema == null))
		{
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (!(rENDERFACADE == null) && rENDERFACADE.AssetCreatureAsync != null)
			{
				rENDERFACADE.AssetCreatureAsync.ChangeHairPrefix(hairPrefix);
			}
		}
	}

	protected void ChangeRenderFacadeOnly(HeroInfo heroInfo)
	{
		if (_heroUid != 0 && _cinema != null)
		{
			NGame2.NRender.NRenderFacade.AnimationData.RuntimeData runtimeData = null;
			VectorXZ direction = new VectorXZ(0f, -1f);
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (rENDERFACADE != null)
			{
				runtimeData = rENDERFACADE.AnimationData.Runtime;
				direction = rENDERFACADE.Action.GetDirection();
			}
			_cinema.DestroyCreature(1);
			SpawnCreature(heroInfo);
			rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (rENDERFACADE != null && runtimeData != null)
			{
				rENDERFACADE.AnimationData.Play(runtimeData.Stock, runtimeData.Frame, null);
				rENDERFACADE.Action.SetDirection(direction, blendMotion: false);
				rENDERFACADE.AnimationData.ChainUpdate();
			}
		}
	}

	protected void ChangeRenderFacadeOnly(PetInfo petInfo)
	{
		if (_heroUid != 0 && _cinema != null)
		{
			NGame2.NRender.NRenderFacade.AnimationData.RuntimeData runtimeData = null;
			VectorXZ direction = new VectorXZ(0f, -1f);
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (rENDERFACADE != null)
			{
				runtimeData = rENDERFACADE.AnimationData.Runtime;
				direction = rENDERFACADE.Action.GetDirection();
			}
			_cinema.DestroyCreature(1);
			SpawnCreature(petInfo);
			rENDERFACADE = _cinema.GETRENDERFACADE(1);
			if (rENDERFACADE != null && runtimeData != null)
			{
				rENDERFACADE.AnimationData.Play(runtimeData.Stock, runtimeData.Frame, null);
				rENDERFACADE.Action.SetDirection(direction, blendMotion: false);
				rENDERFACADE.AnimationData.ChainUpdate();
			}
		}
	}

	protected void SetRenderFacedeAnimation(AnimationKeyData key)
	{
		if (_heroUid != 0 && !(_cinema == null))
		{
			if (_runtimeAnimation != null && !_runtimeAnimation.IsEnd)
			{
				_runtimeAnimation.Stop();
			}
			_runtimeAnimation = VespaCoroutine.StartRuntime(RenderFacadeAnimationAsync(key));
		}
	}

	protected IEnumerator RenderFacadeAnimationAsync(AnimationKeyData key)
	{
		RenderFacade renderFacade = _cinema.GETRENDERFACADE(1);
		if (renderFacade == null || renderFacade.AssetCreatureAsync == null)
		{
			yield break;
		}
		bool creatureAsync = true;
		while (creatureAsync)
		{
			creatureAsync = renderFacade != null && renderFacade.AssetCreatureAsync != null && !renderFacade.AssetCreatureAsync.IsDone;
			yield return null;
		}
		if (renderFacade == null || renderFacade.AssetCreatureAsync == null || !(renderFacade.AssetCreatureAsync.Value is CreatureInstance creatureInstance))
		{
			yield break;
		}
		if (key.MotionType == DefineCreature.EMotion.Idle)
		{
			renderFacade.Action.Idle(blendMotion: false);
			yield break;
		}
		bool flag = false;
		if (creatureInstance.AnimationDic != null)
		{
			flag = creatureInstance.AnimationDic.ContainsKey(key);
		}
		if (flag)
		{
			_motionSequence.Play(key);
		}
		else
		{
			renderFacade.Action.Idle(blendMotion: false);
		}
	}

	public IEnumerator<TaskCurrentData> OpenAsync(PetInfo petInfo, Listener listener, bool useSceneChanger = true)
	{
		return OpenAsyncInternal(SetupPet(petInfo), listener, useSceneChanger);
	}

	public IEnumerator<TaskCurrentData> OpenAsync(CachedHeroInfo heroInfo, Listener listener, bool useSceneChanger = true)
	{
		return OpenAsyncInternal(SetupHero(heroInfo), listener, useSceneChanger);
	}

	private IEnumerator<TaskCurrentData> OpenAsyncInternal(IEnumerator<TaskCurrentData> SetupInfo, Listener listener, bool useSceneChanger = true)
	{
		_sceneChangerFinished = false;
		SceneChanger sceneChanger = null;
		if (useSceneChanger)
		{
			sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
			while (!sceneChanger.IsCompleteAnimation())
			{
				yield return TaskCurrentData.RUNNING;
			}
		}
		_sceneChangerFinished = true;
		bool completeOpen = false;
		Open(delegate
		{
			completeOpen = true;
		});
		while (!completeOpen)
		{
			yield return TaskCurrentData.RUNNING;
		}
		listener.OnOpen();
		_listener = listener;
		using (TaskRunner runner = new TaskRunner(SetupInfo))
		{
			while (runner.MoveNextWhileRunning())
			{
				yield return runner.Current;
			}
		}
		sceneChanger?.Close();
		Vespa_System_Action_Extension_Method.fire(_onEnd);
		_onEnd = null;
		yield return TaskCurrentData.SUCCESS;
	}

	protected IEnumerator<TaskCurrentData> SetupHero(CachedHeroInfo heroInfo)
	{
		HeroInfo = heroInfo;
		CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		return SetupInternal();
	}

	protected IEnumerator<TaskCurrentData> SetupPet(PetInfo petInfo)
	{
		PetInfo = petInfo;
		CreatureData = petInfo.CreatureData;
		return SetupInternal();
	}

	protected IEnumerator<TaskCurrentData> SetupInternal()
	{
		yield return TaskCurrentData.RUNNING;
		InitInfo();
		CloseRenderTargetCinema();
		using (TaskRunner runner = new TaskRunner(InitRenderTargetCinemaAsync()))
		{
			while (runner.MoveNextWhileRunning())
			{
				yield return runner.Current;
			}
		}
		UpdatePauseButtonState();
		UpdateCloseButtonState();
		FirstAction();
		yield return TaskCurrentData.SUCCESS;
	}

	protected override void OnShowRoot(bool isShow)
	{
		base.OnShowRoot(isShow);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_motionSequence != null)
		{
			_motionSequence.ChainUpdate();
		}
		ScrollZoomInOut();
		UpdateHeroCamera();
		UpdateMenuAlpha();
		SprkGraphicsOptions.PollComparisonKeys(this);
	}

	private void LateUpdate()
	{
		if (_cameraController != null)
		{
			_cameraController.ExtraOffsetByDistance = (_photoMode ? PhotoModeExtraOffset : NormalModeExtraOffset);
			_cameraController.ChainLateUpdate();
		}
		SprkFreeCamera.Apply(this);
	}

	public override void Reset()
	{
		base.Reset();
		CloseRenderTargetCinema();
		_listener = null;
		_touchMode = TouchMode.None;
		_touchList.Clear();
		_prevScaleAmount = 0f;
		_photoMode = false;
		SprkFreeCamera.Reset(this);
	}

	public void CloseScene()
	{
		VespaCoroutine.Start(coClose(this));
	}

	private static IEnumerator coClose(HeroFullSceneViewBase This, System.Action onEnd = null)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(10f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			This.Close();
		}
		sceneChanger.Close();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public override void OnClosing()
	{
		if (_motionSequence != null && _motionSequence.IsPaused())
		{
			_motionSequence.Resume();
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
		}
		base.OnClosing();
		if (_listener != null)
		{
			_listener.OnClose();
		}
	}

	RenderFacade MotionSequence.IDataProvider.GetRenderFacade()
	{
		if (_cinema == null)
		{
			return null;
		}
		return _cinema.GETRENDERFACADE(_heroUid);
	}

	private void UpdateMenuAlpha()
	{
		if (!(NormalMenuWidget == null))
		{
			float b = (_photoMode ? 0f : 1f);
			float alpha = NormalMenuWidget.alpha;
			if (!Mathf.Approximately(alpha, b))
			{
				NormalMenuWidget.alpha = Mathf.Lerp(alpha, b, Time.deltaTime * 20f);
			}
		}
	}

	public override void OnSystemBackButton()
	{
		if (!_photoMode)
		{
			OnClickCloseNormal();
		}
		else
		{
			OnClickClosePhoto();
		}
	}

	public virtual void OnClickCloseNormal()
	{
		VespaCoroutine.Start(coClose(this));
	}

	public void CloseAndDoAction(System.Action onClose)
	{
		VespaCoroutine.Start(coClose(this, onClose));
	}

	public virtual void OnClickClosePhoto()
	{
		_photoMode = false;
		UpdateCloseButtonState();
	}

	public void OnClickPrev()
	{
		if (_motionSequence != null)
		{
			_motionSequence.PlayPrev();
			UpdatePauseButtonState();
		}
	}

	public void OnClickNext()
	{
		if (_motionSequence != null)
		{
			_motionSequence.PlayNext();
			UpdatePauseButtonState();
		}
	}

	public void OnTogglePause()
	{
		if (_motionSequence != null)
		{
			if (_motionSequence.IsPaused())
			{
				_motionSequence.Resume();
			}
			else
			{
				_motionSequence.Pause();
			}
			UpdatePauseButtonState();
		}
	}

	protected virtual void UpdatePauseButtonState()
	{
		bool flag = false;
		if (_motionSequence != null)
		{
			flag = _motionSequence.IsPaused();
			if (flag)
			{
				GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.Menu);
			}
			else
			{
				GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.Menu);
			}
		}
		UIHelper.EnableGameObject(ResumeAnimObject, flag);
		UIHelper.EnableGameObject(PauseAnimObject, !flag);
	}

	public void OnClickHide()
	{
		_photoMode = true;
		UpdateCloseButtonState();
	}

	private void UpdateCloseButtonState()
	{
		UIHelper.EnableGameObject(PhotoCloseButton, _photoMode);
		UIHelper.EnableGameObject(NormalCloseButton, !_photoMode);
	}

	protected virtual void InitInfo()
	{
		if ((HeroInfo != null || PetInfo != null) && CreatureData != null)
		{
			UIHelper.SetLabelText(HeroTitleLabel, CreatureData.Title);
			UIHelper.SetLabelText(HeroNameLabel, CreatureData.Name);
			UIHelper.SetLabelText(HeroDescLabel, CreatureData.Description);
		}
	}

	protected virtual void FirstAction()
	{
	}

	private IEnumerator<TaskCurrentData> InitRenderTargetCinemaAsync()
	{
		if (CreatureData == null || (HeroInfo == null && PetInfo == null))
		{
			yield return TaskCurrentData.FAIL;
			yield break;
		}
		RenderTargetCinema.StockData stock = default(RenderTargetCinema.StockData);
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out stock.RenderTargetWidth, out stock.RenderTargetHeight);
		stock.NodeTreePackCode = _nodeTreePackCode;
		stock.NodeTreeName = _nodeTreeName;
		_cinema = RenderTargetCinema.CreateInstance(stock);
		if (_cinema == null)
		{
			yield return TaskCurrentData.FAIL;
			yield break;
		}
		if (HeroInfo != null)
		{
			SpawnCreature(HeroInfo);
		}
		else if (PetInfo != null)
		{
			SpawnCreature(PetInfo);
		}
		do
		{
			yield return TaskCurrentData.RUNNING;
		}
		while (!_cinema.IsCompleteLoad());
		_cinema.StartNodeTree();
		Hero3DTexture.mainTexture = _cinema.RenderTarget;
		_motionSequence = new MotionSequence(this);
		SetCameraController();
		yield return TaskCurrentData.SUCCESS;
	}

	protected virtual void SetCameraController()
	{
		_cameraController = new CameraController(_cinema, _heroUid);
		if (HeroInfo != null)
		{
			_cameraController.SetCameraInfo(HeroInfo.HeroIndex, HeroInfo.CostumeIndex);
		}
		else if (PetInfo != null)
		{
			_cameraController.SetCameraInfo(PetInfo.PetIndex, 0);
		}
		_cameraController.ZoomOutRate = 0.5f;
	}

	private void CloseRenderTargetCinema()
	{
		if (_cinema != null)
		{
			RenderTargetCinema.ReleaseInstance(_cinema);
			_cinema = null;
		}
		_motionSequence = null;
		_cameraController = null;
	}

	private void SpawnCreature(HeroInfo heroInfo)
	{
		if (_cinema == null)
		{
			return;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, heroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		_heroUid = _cinema.SpawnCreature(null, 1, renderFacadeBuilder);
		RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(_heroUid);
		if (rENDERFACADE != null)
		{
			VectorXZ direction = new VectorXZ(0f, -1f);
			ClientCreatureData clientCreatureDataByIndex = DataHelper.GetClientCreatureDataByIndex(heroInfo.HeroIndex, heroInfo.CostumeIndex);
			if (clientCreatureDataByIndex != null)
			{
				direction.x = clientCreatureDataByIndex.LookRotationX;
				direction.z = clientCreatureDataByIndex.LookRotationZ;
			}
			rENDERFACADE.Action.SetDirection(direction, blendMotion: false);
		}
	}

	private void SpawnCreature(PetInfo petInfo)
	{
		if (!(_cinema == null))
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, petInfo);
			renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
			_heroUid = _cinema.SpawnCreature(null, 1, renderFacadeBuilder);
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(_heroUid);
			if (rENDERFACADE != null)
			{
				rENDERFACADE.Action.SetDirection(new VectorXZ(0f, -1f), blendMotion: false);
			}
		}
	}

	private void ScrollZoomInOut()
	{
	}

	private void UpdateHeroCamera()
	{
		if (SprkFreeCamera.InputCamera(this) || _cinema == null || _touchList.Count == 0 || _touchMode == TouchMode.None)
		{
			return;
		}
		if (_touchMode == TouchMode.Rotate && _touchList.Count == 1)
		{
			RenderFacade rENDERFACADE = _cinema.GETRENDERFACADE(_heroUid);
			if (rENDERFACADE != null)
			{
				UICamera.MouseOrTouch mouseOrTouch = _touchList[0];
				VectorXZ direction = rENDERFACADE.Action.GetDirection();
				direction = NVespa.Math.Rotate(direction, mouseOrTouch.delta.x * RotateSensitivity);
				rENDERFACADE.Action.SetDirection(direction, blendMotion: false);
			}
		}
		else if (_touchMode == TouchMode.Scale && _touchList.Count >= 2)
		{
			UICamera.MouseOrTouch mouseOrTouch2 = _touchList[0];
			UICamera.MouseOrTouch mouseOrTouch3 = _touchList[1];
			Vector2 pos = mouseOrTouch2.pos;
			Vector2 pos2 = mouseOrTouch3.pos;
			float num = Vector3.Distance(pos, pos2);
			if (!Mathf.Approximately(_prevScaleAmount, 0f) && _cameraController != null)
			{
				float num2 = (num - _prevScaleAmount) * ScaleSensitivity / 540f;
				_cameraController.ZoomOutRate -= num2;
			}
			_prevScaleAmount = num;
		}
	}

	public virtual void OnTouch()
	{
		if (_motionSequence != null)
		{
			_motionSequence.PlayCurrent();
		}
		UpdatePauseButtonState();
	}

	public void OnPress()
	{
		OnDragBegin();
	}

	public void OnRelease()
	{
		OnDragEnd();
	}

	public virtual void OnDragBegin()
	{
		if (!_touchList.Contains(UICamera.currentTouch))
		{
			_touchList.Add(UICamera.currentTouch);
			_prevScaleAmount = 0f;
			_touchMode = ((_touchList.Count < 2) ? TouchMode.Rotate : TouchMode.Scale);
			_firstTouchPositionForTest = ((_touchList.Count == 1) ? UICamera.currentTouch.pos : _firstTouchPositionForTest);
		}
	}

	public virtual void OnDragEnd()
	{
		if (_touchList.Remove(UICamera.currentTouch))
		{
			_prevScaleAmount = 0f;
			_touchMode = ((_touchList.Count != 0) ? _touchMode : TouchMode.None);
		}
	}

	protected void Zoom(float distance)
	{
		if (_cameraController != null)
		{
			float num = (distance - _prevScaleAmount) * ScaleSensitivity / 540f;
			_cameraController.ZoomOutRate -= num;
		}
	}
}
