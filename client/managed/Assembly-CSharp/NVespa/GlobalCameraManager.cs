using System.Collections.Generic;
using NShader;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public class GlobalCameraManager : GlobalAutoBehaviour<GlobalCameraManager>, IInspectorGuiDrawer
{
	public enum ECameraState
	{
		Normal,
		ScreenFilterTarget,
		DisabledByRequest,
		DisabledByDepthMin
	}

	public class CameraInfo
	{
		public ECameraState State;

		public Camera Cam { get; private set; }

		public CameraInfo(Camera cam)
		{
			Cam = cam;
		}
	}

	private const float MinCameraDepthDefault = -999f;

	private List<Tuple<float, Object>> _minCameraDepthList = new List<Tuple<float, Object>>();

	private List<CameraInfo> _cameraList = new List<CameraInfo>();

	private List<Tuple<Camera, Object>> _disableCameraList = new List<Tuple<Camera, Object>>();

	private bool _shrinkGameCamera;

	private Rect _shrinkGameCameraRect;

	private CameraClearFlags BaseCameraClearFlags => CameraClearFlags.Color;

	public float MinCameraDepth
	{
		get
		{
			float num = -999f;
			for (int i = 0; i < _minCameraDepthList.Count; i++)
			{
				Tuple<float, Object> tuple = _minCameraDepthList[i];
				if (tuple.Item2 == null)
				{
					_minCameraDepthList.RemoveAt(i);
					i--;
				}
				else
				{
					num = Mathf.Max(tuple.Item1, num);
				}
			}
			return num;
		}
	}

	public static List<CameraInfo> CollectAllCameras()
	{
		Camera[] array = Object.FindObjectsOfType<Camera>();
		List<CameraInfo> list = new List<CameraInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			Camera camera = array[i];
			if (camera.cullingMask != 0 && HasMainRenderTarget(camera))
			{
				list.Add(new CameraInfo(array[i]));
			}
		}
		return list;
	}

	public static void SetCameraDepth(string name, float depth)
	{
		foreach (CameraInfo item in CollectAllCameras())
		{
			if (item.Cam.name == name)
			{
				item.Cam.depth = depth;
			}
		}
	}

	public static float GetMostHighDepth()
	{
		List<CameraInfo> list = CollectAllCameras();
		float num = 0f;
		foreach (CameraInfo item in list)
		{
			if (item.Cam.depth > num)
			{
				num = item.Cam.depth;
			}
		}
		return num;
	}

	public static bool HasMainRenderTarget(Camera cam)
	{
		return SprkGraphicsOptions.HasMainRenderTarget(cam);
	}

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("GlobalCameraManager", this, delegate
		{
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("GameCamera");
				if (GUILayout.Button("Shrink", GUILayout.Height(30f)))
				{
					ShrinkGameCamera(new Rect(0.1f, 0.2f, 0.7f, 0.7f));
				}
				if (GUILayout.Button("Shrink2", GUILayout.Height(30f)))
				{
					ShrinkGameCamera(new Rect(0.05f, 0.05f, 0.9f, 0.9f));
				}
				if (GUILayout.Button("Restore", GUILayout.Height(30f)))
				{
					RestoreGameCamera();
				}
			}
		});
	}

	public bool PushMinCameraDepth(int layer, Object obj)
	{
		if (!TryGetCameraDepthByLayer(layer, out var cameraDepth))
		{
			return false;
		}
		float minCameraDepth = MinCameraDepth;
		_minCameraDepthList.Add(Tuple.Create(cameraDepth, obj));
		float minCameraDepth2 = MinCameraDepth;
		if (Mathf.Approximately(minCameraDepth, minCameraDepth2))
		{
			return true;
		}
		AdjustCameraRenderAndClearFlags();
		return true;
	}

	public void PopMinCameraDepth(Object obj)
	{
		float minCameraDepth = MinCameraDepth;
		for (int i = 0; i < _minCameraDepthList.Count; i++)
		{
			if (_minCameraDepthList[i].Item2 == obj)
			{
				_minCameraDepthList.RemoveAt(i);
				i--;
			}
		}
		float minCameraDepth2 = MinCameraDepth;
		if (!Mathf.Approximately(minCameraDepth, minCameraDepth2))
		{
			AdjustCameraRenderAndClearFlags();
		}
	}

	public void DisableCamera(Camera cam, Object caller, bool autoAdjust = true)
	{
		for (int i = 0; i < _disableCameraList.Count; i++)
		{
			if (_disableCameraList[i].Item1 == cam && _disableCameraList[i].Item2 == caller)
			{
				return;
			}
		}
		_disableCameraList.Add(Tuple.Create(cam, caller));
		if (autoAdjust)
		{
			AdjustCameraRenderAndClearFlags();
		}
	}

	public void EnableCamera(Camera cam, Object caller, bool autoAdjust = true)
	{
		for (int i = 0; i < _disableCameraList.Count; i++)
		{
			if (_disableCameraList[i].Item1 == cam && _disableCameraList[i].Item2 == caller)
			{
				_disableCameraList.RemoveAt(i);
				i--;
			}
		}
		if (autoAdjust)
		{
			AdjustCameraRenderAndClearFlags();
		}
	}

	public bool IsDisabledCam(Camera cam)
	{
		for (int i = 0; i < _disableCameraList.Count; i++)
		{
			if (_disableCameraList[i].Item1 == cam)
			{
				return true;
			}
		}
		return false;
	}

	public Camera GetCameraByLayer(int layerIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_cameraList))
		{
			return null;
		}
		for (int i = 0; i < _cameraList.Count; i++)
		{
			CameraInfo cameraInfo = _cameraList[i];
			if (!(cameraInfo.Cam == null) && cameraInfo.Cam.gameObject.layer == layerIndex)
			{
				return cameraInfo.Cam;
			}
		}
		return null;
	}

	public float GetCameraDepthByLayer(int layerIndex)
	{
		if (TryGetCameraDepthByLayer(layerIndex, out var cameraDepth))
		{
			return cameraDepth;
		}
		return 0f;
	}

	public bool TryGetCameraDepthByLayer(int layerIndex, out float cameraDepth)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_cameraList))
		{
			cameraDepth = 0f;
			return false;
		}
		for (int i = 0; i < _cameraList.Count; i++)
		{
			CameraInfo cameraInfo = _cameraList[i];
			if (!(cameraInfo.Cam == null) && cameraInfo.Cam.gameObject.layer == layerIndex)
			{
				cameraDepth = cameraInfo.Cam.depth;
				return true;
			}
		}
		cameraDepth = 0f;
		return false;
	}

	public void AdjustCameraRenderAndClearFlags()
	{
		if (!AdjustCameraRenderAndClearFlags(MinCameraDepth))
		{
			_minCameraDepthList.Clear();
			AdjustCameraRenderAndClearFlags(MinCameraDepth);
		}
	}

	public void ShrinkGameCamera(Rect rect)
	{
		if (Mathf.Approximately(rect.x, 0f) && Mathf.Approximately(rect.y, 0f) && Mathf.Approximately(rect.width, 1f) && Mathf.Approximately(rect.height, 1f))
		{
			_shrinkGameCamera = false;
		}
		else
		{
			_shrinkGameCamera = true;
			_shrinkGameCameraRect = rect;
		}
		AdjustCameraRenderAndClearFlags();
	}

	public void RestoreGameCamera()
	{
		_shrinkGameCamera = false;
		AdjustCameraRenderAndClearFlags();
	}

	public RenderTexture CaptureCamera(float maxOverCameraDepth)
	{
		RenderTexture renderTexture = FilterBase.CreateScreenRenderTarget();
		renderTexture.DiscardContents();
		if (!CaptureCamera(maxOverCameraDepth, renderTexture))
		{
			VespaObject.DestroyObject(renderTexture);
		}
		return renderTexture;
	}

	public RenderTexture CaptureCamera(float maxOverCameraDepth, IFilter filter)
	{
		if (filter == null)
		{
			return CaptureCamera(maxOverCameraDepth);
		}
		SingletonAuto<ResolutionInfo>.instance.GetScreenRenderTargetSize(out var width, out var height);
		RenderTexture temporaryRT = FilterBase.GetTemporaryRT(width, height, 24, RenderTextureFormat.ARGB32);
		temporaryRT.DiscardContents();
		if (!CaptureCamera(maxOverCameraDepth, temporaryRT))
		{
			RenderTexture.ReleaseTemporary(temporaryRT);
			return null;
		}
		RenderTexture renderTexture = FilterBase.CreateRenderTargetWithAntiAliasing(width, height, 0, 1);
		filter.ImageProcess(temporaryRT, renderTexture, discardDest: true);
		RenderTexture.ReleaseTemporary(temporaryRT);
		return renderTexture;
	}

	public bool CaptureCamera(float maxOverCameraDepth, RenderTexture rt)
	{
		AdjustCameraRenderAndClearFlags();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_cameraList))
		{
			return false;
		}
		RenderTexture.active = rt;
		for (int i = 0; i < _cameraList.Count; i++)
		{
			CameraInfo cameraInfo = _cameraList[i];
			if (!(cameraInfo.Cam == null))
			{
				if (cameraInfo.Cam.depth >= maxOverCameraDepth)
				{
					break;
				}
				RenderTexture targetTexture = cameraInfo.Cam.targetTexture;
				Rect rect = cameraInfo.Cam.rect;
				cameraInfo.Cam.targetTexture = rt;
				cameraInfo.Cam.rect = new Rect(0f, 0f, 1f, 1f);
				cameraInfo.Cam.Render();
				cameraInfo.Cam.targetTexture = targetTexture;
				cameraInfo.Cam.rect = rect;
			}
		}
		RenderTexture.active = null;
		return true;
	}

	public bool HasScreenFilterTargetCamera()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_cameraList))
		{
			return false;
		}
		for (int i = 0; i < _cameraList.Count; i++)
		{
			if (_cameraList[i].State == ECameraState.ScreenFilterTarget)
			{
				return true;
			}
		}
		return false;
	}

	private bool AdjustCameraRenderAndClearFlags(float cameraDepthMin)
	{
		for (int i = 0; i < _disableCameraList.Count; i++)
		{
			if (_disableCameraList[i].Item1 == null)
			{
				_disableCameraList.RemoveAt(i);
				i--;
			}
		}
		RefreshCamera();
		List<CameraInfo> list = new List<CameraInfo>(_cameraList);
		for (int j = 0; j < list.Count; j++)
		{
			CameraInfo cameraInfo = list[j];
			if (IsDisabledCam(cameraInfo.Cam))
			{
				cameraInfo.State = ECameraState.DisabledByRequest;
				cameraInfo.Cam.enabled = false;
				list.RemoveAt(j);
				j--;
			}
		}
		bool flag = false;
		for (int k = 0; k < list.Count; k++)
		{
			CameraInfo cameraInfo2 = list[k];
			if (cameraInfo2.Cam.depth < cameraDepthMin)
			{
				cameraInfo2.State = ECameraState.DisabledByDepthMin;
				cameraInfo2.Cam.targetTexture = null;
				cameraInfo2.Cam.enabled = false;
				continue;
			}
			cameraInfo2.Cam.enabled = true;
			cameraInfo2.State = ECameraState.Normal;
			cameraInfo2.Cam.targetTexture = null;
			if (_shrinkGameCamera)
			{
				cameraInfo2.Cam.rect = _shrinkGameCameraRect;
			}
			else
			{
				cameraInfo2.Cam.rect = new Rect(0f, 0f, 1f, 1f);
			}
			if (!flag)
			{
				flag = true;
				cameraInfo2.Cam.clearFlags = BaseCameraClearFlags;
				if (SystemStatus.IsPlayingCritical)
				{
					cameraInfo2.Cam.backgroundColor = Color.black;
				}
			}
			else
			{
				cameraInfo2.Cam.clearFlags = CameraClearFlags.Depth;
			}
		}
		return true;
	}

	private void RefreshCamera()
	{
		_cameraList = CollectAllCameras();
		_cameraList.Sort((CameraInfo lhs, CameraInfo rhs) => lhs.Cam.depth.CompareTo(rhs.Cam.depth));
	}

	public void OnSceneChanged()
	{
	}
}
