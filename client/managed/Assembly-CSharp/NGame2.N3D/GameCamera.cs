using System;
using NGame2.NNodeTree.NSceneContainer;
using NGameAsset.NBattleFieldMap2;
using NShared;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.N3D;

[ExecuteInEditMode]
public class GameCamera : MonoBehaviour, ICamera, IInspectorGuiDrawer
{
	public interface IView
	{
		bool GetCameraView(CameraStat.CameraStock cameraStock, out Vector3 posOnCameraSpace);
	}

	public EventCamera EventCamera = new EventCamera();

	[HideInInspector]
	public Game3DRoot.LayerManager LayerManager = new Game3DRoot.LayerManager();

	public float ScreenUpsideSpace;

	public float ScreenDownsideSpace;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private Camera _battleUICamera;

	private PostProcessVolume _postProcessVolume;

	private ShakeMaker _cameraShake;

	private FieldCameraDataChanger _fieldCameraDataChanger = new FieldCameraDataChanger();

	private Vector3 _originalPos;

	private Quaternion _originalRot;

	public Camera MainCamera => _camera;

	public Camera BattleUICamera => _battleUICamera;

	public IView View { get; set; }

	public ShakeMaker CameraShake => _cameraShake;

	public float FovH => Mathf.Atan(Mathf.Tan(_camera.fieldOfView * 0.5f * (MathF.PI / 180f)) * (float)SingletonAuto<ResolutionInfo>.instance.RealScreenWidth / (float)SingletonAuto<ResolutionInfo>.instance.RealScreenHeight) * 2f * 57.29578f;

	Camera ICamera.GetUnityCamera()
	{
		return MainCamera;
	}

	ShakeMaker ICamera.GetCameraShake()
	{
		return CameraShake;
	}

	void ICamera.SetAlphaToggleEntry(AlphaToggleEntry entry)
	{
		EventCamera.SetAlphaToggleEntry(entry);
	}

	void ICamera.SetAlphaToggleEntry2(AlphaToggleEntry2 entry)
	{
		EventCamera.SetAlphaToggleEntry2(entry);
	}

	void ICamera.SetFocusEntry(IFocusCameraEntry entry)
	{
		EventCamera.SetFocusEntry(entry);
	}

	void ICamera.SetEventEntry(IEventCameraEntry entry)
	{
		EventCamera.SetEntry(entry);
	}

	void ICamera.ReleaseEventEntry(IEventCameraEntry entry)
	{
		EventCamera.ReleaseEntry(entry);
	}

	public void Init()
	{
		if (MainCamera != null)
		{
			_originalPos = MainCamera.transform.position;
			_originalRot = MainCamera.transform.rotation;
		}
		LayerInfo layerInfo = GetLayerInfo(Game3DRoot.ELayerType.Default);
		_camera = layerInfo.Camera;
		LayerInfo layerInfo2 = GetLayerInfo(Game3DRoot.ELayerType.BattleUI);
		_battleUICamera = layerInfo2.Camera;
		base.transform.localRotation = Quaternion.AngleAxis(CameraStat.Basic.LookAngle, Vector3.right);
	}

	public void RestoreMainCamera()
	{
		MainCamera.transform.position = _originalPos;
		MainCamera.transform.rotation = _originalRot;
	}

	public LayerInfo GetLayerInfo(Game3DRoot.ELayerType layerType)
	{
		if (LayerManager.TryGetLayerInfo(layerType, out var resultLayerInfo))
		{
			return resultLayerInfo;
		}
		resultLayerInfo = LayerManager.GetLayerInfo(base.gameObject, layerType);
		resultLayerInfo.Camera.transform.localRotation = CameraStat.Basic.CameraRotation;
		resultLayerInfo.Camera.transform.localPosition = CameraStat.Basic.CameraRotation * (Vector3.back * 20f);
		return resultLayerInfo;
	}

	private void Start()
	{
		LayerManager.AdjustCameraDepthAndClearFlags();
		_cameraShake = new ShakeMaker();
		_ = _camera != null;
		SceneAutoBehaviour<GuiManager>.instance.Add("GameCamera", this, DrawToggleGui);
	}

	public void SetCameraFarClipPlane(int farclipPlane)
	{
		if (!(_camera == null) && farclipPlane != 0)
		{
			_camera.farClipPlane = farclipPlane;
		}
	}

	public void SetupPostPorcessing(BattleType battleType)
	{
		if (_camera == null)
		{
			VespaDebug.LogError("Camera not found");
			return;
		}
		_postProcessVolume = Vespa_Unity_Component_Extension_Method.GetComponentAuto<PostProcessVolume>(_camera);
		_postProcessVolume.enabled = SystemOptionData.Render_EnableRimLight;
		_postProcessVolume.isGlobal = true;
		string i_path = "Effect/PostProcessBattle";
		switch (battleType)
		{
		case BattleType.Eclipse:
			i_path = "Effect/PostProcessEclipse";
			break;
		case BattleType.GodkingTrial:
			i_path = "Effect/PostProcessGodkingTrial";
			break;
		case BattleType.Maze:
			i_path = "Effect/PostProcessMaze";
			break;
		}
		PostProcessProfile postProcessProfile = VespaResource.Load<PostProcessProfile>(i_path);
		if (postProcessProfile != null)
		{
			_postProcessVolume.profile = postProcessProfile;
		}
	}

	public void ChainUpdate()
	{
		UpdateInternal(Time.frameCount);
	}

	private void UpdateInternal(int count)
	{
		UpdatePlaying();
	}

	public void ChangeFieldCameraData(string cameraName = "", bool immediately = false)
	{
		_fieldCameraDataChanger.ChangeFieldCameraData(cameraName, immediately);
	}

	public void ChangeFieldCameraData(FieldCameraData cameraData, bool immediately = false)
	{
		_fieldCameraDataChanger.ChangeFieldCameraData(cameraData, immediately);
	}

	private void UpdatePlaying()
	{
		if (View != null && _cameraShake != null)
		{
			CameraStat.CameraStock cameraStock = _fieldCameraDataChanger.UpdateChangeFieldCameraData();
			if (View.GetCameraView(cameraStock, out var posOnCameraSpace))
			{
				Vector3 cameraPos = cameraStock.CameraRotation * posOnCameraSpace;
				Quaternion cameraRot = cameraStock.CameraRotation;
				float cameraFov = cameraStock.FieldOfView;
				EventCamera.ChainUpdate(ref cameraPos, ref cameraRot, ref cameraFov);
				_cameraShake.ApplyShake(ref cameraPos, EventCamera.Value.ShakeWeight);
				_camera.transform.position = cameraPos;
				_camera.transform.rotation = cameraRot;
				_camera.fieldOfView = cameraFov;
				_battleUICamera.transform.position = cameraPos;
				_battleUICamera.transform.rotation = cameraRot;
				_battleUICamera.fieldOfView = cameraFov;
			}
		}
	}

	public void DrawToggleGui()
	{
		if (_postProcessVolume == null)
		{
			GUILayout.Label("Post Porcessing does not exist.");
			return;
		}
		_postProcessVolume.enabled = GUILayout.Toggle(_postProcessVolume.enabled, "Enable post processing");
		if (!_postProcessVolume.enabled)
		{
			return;
		}
		PostProcessProfile profile = _postProcessVolume.profile;
		foreach (PostProcessEffectSettings setting2 in profile.settings)
		{
			setting2.enabled.value = GUILayout.Toggle(setting2.enabled, setting2.name);
		}
		ChromaticAberration setting = profile.GetSetting<ChromaticAberration>();
		if (!(setting != null))
		{
			return;
		}
		GUILayout.Label("Chromatic Aberration Intensity");
		using (new BlockManager().Horizontal())
		{
			GUILayout.Space(30f);
			setting.intensity.value = GUILayout.HorizontalSlider(setting.intensity, 0f, 1f);
		}
	}
}
