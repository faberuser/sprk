using System;
using System.Collections.Generic;
using NGame2.NDevice;
using NGame2.NGameRoot;
using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2;

public class GlobalUIRoot : MonoBehaviour, IInspectorGuiDrawer
{
	public interface ITouchListener
	{
		void OnTouch(UICamera.MouseOrTouch touch);

		void OnDrag(UICamera.MouseOrTouch touch);

		void OnPress(UICamera.MouseOrTouch touch, bool press);

		void OnRelease();
	}

	public enum ELayerType
	{
		[GameLayer(Depth = 20, IsUI = true)]
		GlobalVideo,
		[GameLayer(Depth = 21, IsUI = true)]
		GlobalUI,
		[GameLayer(Depth = 22, IsUI = true)]
		Global3D
	}

	[Serializable]
	public class LayerManager : GenericLayerManager<ELayerType>
	{
	}

	public const int UIWidth = 1920;

	public const int UIHeight = 1080;

	private static GlobalUIRoot _instance;

	[HideInInspector]
	public UIRoot UIRoot;

	[HideInInspector]
	public GameObject CameraGroup;

	[HideInInspector]
	public GameObject ContentsGroup;

	[HideInInspector]
	[SerializeField]
	private LayerManager _layerManager = new LayerManager();

	private List<WeakRef<ITouchListener>> _touchListenerList = new List<WeakRef<ITouchListener>>();

	public static GlobalUIRoot instance
	{
		get
		{
			if (_instance == null)
			{
				CreateInstance();
			}
			return _instance;
		}
	}

	public static bool isValidInstance => _instance != null;

	[HideInInspector]
	public LayerInfo UI => GetLayerInfo(ELayerType.GlobalUI);

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			GameObject obj = NGUITools.AddChild(null, undo: false);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.layer = LayerMask.NameToLayer(EnumTools.GetName(ELayerType.GlobalUI));
			obj.name = "Global UI Root";
			_instance = obj.AddComponent<GlobalUIRoot>();
			_instance.CreateUIRoot();
		}
	}

	public static void OnDrag(UICamera.MouseOrTouch touch)
	{
		instance.ProcessDrag(touch);
	}

	public static void OnPress(UICamera.MouseOrTouch touch, bool press)
	{
		instance.ProcessPress(touch, press);
	}

	public static void OnTouch(UICamera.MouseOrTouch touch)
	{
		Camera camera = instance.UI.Camera;
		Vespa_Unity_Camera_Extension_Method.VespaScreenToViewportPoint(camera, touch.pos);
		Vector3 vector = Vespa_Unity_Camera_Extension_Method.VespaScreenToWorldPoint(camera, touch.pos);
		Vector4 vector2 = instance.ContentsGroup.transform.worldToLocalMatrix * vector;
		GlobalTopmostWindow.instance.TouchEffect(vector2);
		SingletonAuto<SleepModeManager>.instance.Touch();
		instance.ProcessTouch(touch);
	}

	public void CreateUIRoot()
	{
		UIRoot = base.gameObject.GetComponent<UIRoot>();
		if (!(UIRoot != null))
		{
			UIRoot = base.gameObject.AddComponent<UIRoot>();
			UIRoot.scalingStyle = UIRoot.Scaling.Constrained;
			UIRoot.fitWidth = true;
			UIRoot.fitHeight = true;
			UIRoot.manualWidth = 1920;
			UIRoot.manualHeight = 1080;
			CameraGroup = NGUITools.AddChild(UIRoot.gameObject, undo: false);
			CameraGroup.name = "Camera Group";
			ContentsGroup = NGUITools.AddChild(UIRoot.gameObject, undo: false);
			ContentsGroup.name = "Contents Group";
		}
	}

	public LayerInfo GetLayerInfo(GameObject cameraGroup, ELayerType layerType)
	{
		return _layerManager.GetLayerInfo(cameraGroup, layerType);
	}

	public LayerInfo GetLayerInfo(ELayerType layerType)
	{
		return GetLayerInfo(CameraGroup, layerType);
	}

	public LayerInfo GetLayerInfo(int layer)
	{
		return _layerManager.GetLayerInfo(layer);
	}

	public void OnSceneChanged()
	{
		_layerManager.RebuildLayer();
	}

	public bool RefreshCameraEnable()
	{
		if (ContentsGroup == null)
		{
			return false;
		}
		bool result = false;
		bool flag = false;
		Transform transform = ContentsGroup.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i).gameObject.activeInHierarchy)
			{
				flag = true;
				break;
			}
		}
		for (int j = 0; j < _layerManager.Count; j++)
		{
			LayerInfo layerInfo = _layerManager[j];
			if (layerInfo.IsContentsExist != flag)
			{
				if (flag)
				{
					GlobalAutoBehaviour<GlobalCameraManager>.instance.EnableCamera(layerInfo.Camera, this, autoAdjust: false);
				}
				else
				{
					GlobalAutoBehaviour<GlobalCameraManager>.instance.DisableCamera(layerInfo.Camera, this, autoAdjust: false);
				}
				layerInfo.IsContentsExist = flag;
				result = true;
			}
		}
		return result;
	}

	public void AddTouchListener(ITouchListener listener)
	{
		if (_touchListenerList.FindIndex((WeakRef<ITouchListener> e) => e.Value == listener) == -1)
		{
			WeakRef<ITouchListener> weakRef = new WeakRef<ITouchListener>();
			weakRef.Value = listener;
			_touchListenerList.Add(weakRef);
		}
	}

	public bool RemoveTouchListener(ITouchListener listener)
	{
		return _touchListenerList.RemoveAll((WeakRef<ITouchListener> e) => e.Value == listener) > 0;
	}

	private void ProcessDrag(UICamera.MouseOrTouch touch)
	{
		ProcessMouseOrTouchAction(delegate(ITouchListener e)
		{
			e.OnDrag(touch);
		});
	}

	private void ProcessTouch(UICamera.MouseOrTouch touch)
	{
		ProcessMouseOrTouchAction(delegate(ITouchListener e)
		{
			e.OnTouch(touch);
		});
	}

	private void ProcessPress(UICamera.MouseOrTouch touch, bool press)
	{
		ProcessMouseOrTouchAction(delegate(ITouchListener e)
		{
			e.OnPress(touch, press);
		});
	}

	private void ProcessMouseOrTouchAction(Action<ITouchListener> action)
	{
		List<WeakRef<ITouchListener>> list = null;
		foreach (WeakRef<ITouchListener> touchListener in _touchListenerList)
		{
			if (touchListener != null && touchListener.Value == null)
			{
				list = list ?? new List<WeakRef<ITouchListener>>();
				list.Add(touchListener);
			}
		}
		if (list != null)
		{
			list.ForEach(delegate(WeakRef<ITouchListener> e)
			{
				_touchListenerList.Remove(e);
			});
			list.Clear();
		}
		foreach (WeakRef<ITouchListener> touchListener2 in _touchListenerList)
		{
			Vespa_System_Action_Extension_Method.fire(action, touchListener2.Value);
		}
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
		_layerManager.AdjustCameraDepthAndClearFlags();
		if (ClientDebugStatistics.OpenStatistics.Value)
		{
			ClientDebugStatistics.Open();
		}
		GlobalAutoBehaviour<GuiManager>.instance.Add("GlobalUIRoot", this, delegate
		{
			GUILayout.Label($"UIDrawCall.MeshCreateCount={UIDrawCall.MeshCreateCount}");
			GUILayout.Label($"UIDrawCall.MeshDestroyCount={UIDrawCall.MeshDestroyCount}");
			GUILayout.Label($"UIDrawCall.Mesh Remained={UIDrawCall.MeshCreateCount - UIDrawCall.MeshDestroyCount}");
			if (GUILayout.Button("Client Debug Statistics"))
			{
				if (ClientDebugStatistics.IsOpen())
				{
					ClientDebugStatistics.Close();
				}
				else
				{
					ClientDebugStatistics.Open();
				}
			}
			GUILayout.Label("Statistics Alpha");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Space(30f);
				ClientDebugStatistics.StatisticsAlpha.Value = GUILayout.HorizontalSlider(ClientDebugStatistics.StatisticsAlpha.Value, 0f, 1f);
			}
			if (GUILayout.Button("Rebuild Camera"))
			{
				_layerManager.RebuildLayer();
			}
		});
	}

	private void Update()
	{
		MainTimer.Refresh();
	}
}
