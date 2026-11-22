using System;
using System.Collections.Generic;
using NGame2.NGameRoot;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2;

public class GameUIRoot : MonoBehaviour, IInspectorGuiDrawer
{
	public enum ELayerType
	{
		[GameLayer(Depth = 10, IsUI = true)]
		UI,
		[GameLayer(Depth = 11, IsUI = true)]
		UI3D,
		[GameLayer(Depth = 12, IsUI = true)]
		UI3DUI,
		[GameLayer(Depth = 1, IsUI = true)]
		BottomUI,
		[GameLayer(Depth = 2, IsUI = true)]
		BottomFixedUI,
		[GameLayer(Depth = 3, IsUI = true)]
		Bottom3D,
		[GameLayer(Depth = 14, IsUI = true)]
		CutSceneBottomUI,
		[GameLayer(Depth = 15, IsUI = true)]
		CutScene3D,
		[GameLayer(Depth = 16, IsUI = true)]
		CutSceneUI,
		[GameLayer(Depth = -9, IsUI = true)]
		BattleBackgroundUI,
		[GameLayer(Depth = 4, IsUI = true)]
		Bottom3DUI,
		[GameLayer(Depth = 5, IsUI = true)]
		Pet
	}

	[Serializable]
	public class LayerManager : GenericLayerManager<ELayerType>
	{
	}

	public const int UIWidth = 1920;

	public const int UIHeight = 1080;

	private static GameUIRoot _instance;

	[HideInInspector]
	public UIRoot UIRoot;

	[HideInInspector]
	public GameObject CameraGroup;

	[HideInInspector]
	public GameObject ContentsGroup;

	[HideInInspector]
	public UIPanel TopmostPanel;

	[HideInInspector]
	[SerializeField]
	private LayerManager _layerManager = new LayerManager();

	private WindowPrefab<BackgroundBlack> _bgBlackInstance = new WindowPrefab<BackgroundBlack>("Resources/Prefab2/Window/BackgroundBlack", ELayerType.UI);

	private static ELayerType[] BottomLayerArray = new ELayerType[2]
	{
		ELayerType.Bottom3D,
		ELayerType.BottomUI
	};

	private static ELayerType[] CutSceneLayerArray = new ELayerType[3]
	{
		ELayerType.CutSceneBottomUI,
		ELayerType.CutScene3D,
		ELayerType.CutSceneUI
	};

	private List<LayerInfo> _tempLayerInfoList = new List<LayerInfo>();

	public static bool IsValidInstance => _instance != null;

	public static GameUIRoot instance
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

	[HideInInspector]
	public LayerInfo UI => GetLayerInfo(ELayerType.UI);

	[HideInInspector]
	public LayerInfo WorldMap => GetLayerInfo(ELayerType.BottomUI);

	public BackgroundBlack BgBlack => _bgBlackInstance.instance;

	private static void CreateInstance()
	{
		GameUIRoot gameUIRoot = UnityEngine.Object.FindObjectOfType<GameUIRoot>();
		if (gameUIRoot != null)
		{
			_instance = gameUIRoot;
			return;
		}
		int num = -1;
		if (num == -1)
		{
			num = LayerMask.NameToLayer(EnumTools.GetName(ELayerType.UI));
		}
		GameObject gameObject = NGUITools.AddChild(null, undo: false);
		if (num != -1)
		{
			gameObject.layer = num;
		}
		gameObject.name = "Game UI Root";
		_instance = gameObject.AddComponent<GameUIRoot>();
		_instance.CreateUIRoot();
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

	public bool RefreshCameraEnable()
	{
		if (ContentsGroup == null)
		{
			return false;
		}
		return (byte)(0u | (RefreshLayerGroupEnable(BottomLayerArray) ? 1u : 0u) | (RefreshLayerGroupEnable(CutSceneLayerArray) ? 1u : 0u)) != 0;
	}

	private bool RefreshLayerGroupEnable(ELayerType[] layerGroup)
	{
		bool result = false;
		bool flag = false;
		_tempLayerInfoList.Clear();
		for (int i = 0; i < layerGroup.Length; i++)
		{
			if (_layerManager.TryGetLayerInfo(layerGroup[i], out var resultLayerInfo))
			{
				_tempLayerInfoList.Add(resultLayerInfo);
			}
		}
		if (_tempLayerInfoList.Count == 0)
		{
			return false;
		}
		Transform transform = ContentsGroup.transform;
		for (int j = 0; j < transform.childCount; j++)
		{
			GameObject gameObject = transform.GetChild(j).gameObject;
			if (!gameObject.activeInHierarchy)
			{
				continue;
			}
			bool flag2 = false;
			for (int k = 0; k < _tempLayerInfoList.Count; k++)
			{
				if (_tempLayerInfoList[k].Layer == gameObject.layer)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				flag = true;
				break;
			}
		}
		for (int l = 0; l < _tempLayerInfoList.Count; l++)
		{
			LayerInfo layerInfo = _tempLayerInfoList[l];
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

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
		int occlusionLayer = LayerMask.NameToLayer(Vespa_String_Extension_Method.GetEnumString(ELayerType.UI));
		SingletonAuto<OcclusionManager>.instance.SetOcclusionLayer(occlusionLayer);
		_layerManager.AdjustCameraDepthAndClearFlags();
	}

	private void Update()
	{
		MainTimer.Refresh();
	}
}
