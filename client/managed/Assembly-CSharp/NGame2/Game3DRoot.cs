using System;
using NGame2.N3D;
using NGame2.NGameRoot;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2;

public class Game3DRoot : MonoBehaviour, IInspectorGuiDrawer
{
	public enum ELayerType
	{
		[GameLayer(Depth = 0, IsUI = false)]
		Default,
		[GameLayer(Depth = 29, IsUI = false)]
		BattleUI
	}

	[Serializable]
	public class LayerManager : GenericLayerManager<ELayerType>
	{
	}

	private static Game3DRoot _instance;

	[HideInInspector]
	public GameObject CameraGroup;

	[HideInInspector]
	public GameObject ContentsGroup;

	[HideInInspector]
	public GameCamera GameCamera;

	public static int DefaultLayer => LayerMask.NameToLayer(EnumTools.GetName(ELayerType.Default));

	public static int BattleUILayer => LayerMask.NameToLayer(EnumTools.GetName(ELayerType.BattleUI));

	public static bool IsValidInstance => _instance != null;

	public static Game3DRoot instance
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

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			int num = LayerMask.NameToLayer(EnumTools.GetName(ELayerType.Default));
			GameObject gameObject = new GameObject();
			if (num != -1)
			{
				gameObject.layer = num;
			}
			gameObject.name = "Game 3D Root";
			_instance = gameObject.AddComponent<Game3DRoot>();
			_instance.Reserve3DRoot();
		}
	}

	private void Reserve3DRoot()
	{
		if (CameraGroup == null)
		{
			CameraGroup = NGUITools.AddChild(base.gameObject, undo: false);
			CameraGroup.name = "Camera Group";
		}
		if (ContentsGroup == null)
		{
			ContentsGroup = NGUITools.AddChild(base.gameObject, undo: false);
			ContentsGroup.name = "Contents Group";
		}
		if (GameCamera == null)
		{
			GameObject gameObject = new GameObject("GameCamera");
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, CameraGroup.transform);
			GameCamera = gameObject.AddComponent<GameCamera>();
			GameCamera.Init();
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Start()
	{
	}

	public void ChainUpdate()
	{
		if (GameCamera != null)
		{
			GameCamera.ChainUpdate();
		}
	}

	public bool RefreshCameraEnable()
	{
		if (ContentsGroup == null)
		{
			return false;
		}
		bool result = false;
		Transform transform = ContentsGroup.transform;
		for (int i = 0; i < GameCamera.LayerManager.Count; i++)
		{
			LayerInfo layerInfo = GameCamera.LayerManager[i];
			bool flag = false;
			for (int j = 0; j < transform.childCount; j++)
			{
				GameObject gameObject = transform.GetChild(j).gameObject;
				if (gameObject.layer == layerInfo.Layer && gameObject.activeInHierarchy)
				{
					flag = true;
					break;
				}
			}
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
}
