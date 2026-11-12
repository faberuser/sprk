using NGame2.NScene.NPreloader;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI;

public class WindowPrefab<T> : IWindowPrefab where T : Component, IWindow
{
	private PrefabAuto<T> _prefabAuto;

	private WeakRef<T> _instance = new WeakRef<T>();

	private GameUIRoot.ELayerType _layerType;

	public T instance
	{
		get
		{
			if (!IsValidInstance)
			{
				CreateInstance();
			}
			return _instance.Value;
		}
	}

	public bool IsValidInstance => !Vespa_Common_Class_Extension_Method.IsUnityNull(_instance.Value);

	public IWindow WindowRaw => _instance.Value;

	public T Prefab
	{
		get
		{
			if (_prefabAuto != null && _prefabAuto.Value != null && _prefabAuto.Value.LoadGo != null)
			{
				return _prefabAuto.Value.LoadGo.GetComponent<T>();
			}
			return null;
		}
	}

	public WindowPrefab(string path, GameUIRoot.ELayerType layerType)
	{
		_prefabAuto = new PrefabAuto<T>(path);
		_layerType = layerType;
	}

	public void ReserveCache(UiPreloader preloader)
	{
		preloader.AddPrefabLoad(_prefabAuto.ReserveAsync(), delegate
		{
			if (_prefabAuto.Value != null)
			{
				T component = _prefabAuto.Value.LoadGo.GetComponent<T>();
				if (component != null)
				{
					component.ReserveCache(preloader);
				}
			}
		});
	}

	public void CreateInstance(GameUIRoot gameUiRoot = null)
	{
		if (IsValidInstance)
		{
			return;
		}
		if (_prefabAuto.Factory == null)
		{
			_ = _prefabAuto.Factory;
			return;
		}
		if (gameUiRoot == null)
		{
			gameUiRoot = GameUIRoot.instance;
		}
		LayerInfo layerInfo = gameUiRoot.GetLayerInfo(_layerType);
		_instance.Value = _prefabAuto.Factory.Create(gameUiRoot.ContentsGroup.transform, layerInfo.Camera.gameObject.layer);
	}

	public void ChangeLayerRecursive(GameUIRoot.ELayerType layerType)
	{
		if (_instance != null && !(_instance.Value == null) && _layerType != layerType)
		{
			_layerType = layerType;
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(_layerType);
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_instance.Value.gameObject, layerInfo.Layer);
		}
	}

	public bool IsOpen()
	{
		if (!IsValidInstance)
		{
			return false;
		}
		return _instance.Value.IsOpen();
	}

	public void CloseWindow()
	{
		if (IsValidInstance)
		{
			_instance.Value.Close();
		}
	}

	public void ReleaseWindow()
	{
		if (IsValidInstance)
		{
			if (_instance.Value.IsOpen())
			{
				_instance.Value.Close();
			}
			FactoryBase.ReleaseComponent(_instance.Value);
			_instance.Value = null;
		}
	}
}
