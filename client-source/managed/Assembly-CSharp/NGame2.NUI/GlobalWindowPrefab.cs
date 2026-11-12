using NGame2.NScene.NPreloader;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI;

public class GlobalWindowPrefab<T> : IGlobalWindowPrefab where T : Component, IGlobalWindow
{
	private PrefabAuto<T> _prefabAuto;

	private T _instance;

	private GlobalUIRoot.ELayerType _layerType;

	public T instance
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

	public bool IsValidInstance => !Vespa_Common_Class_Extension_Method.IsUnityNull(_instance);

	public IGlobalWindow WindowRaw => _instance;

	public GlobalWindowPrefab(string path, GlobalUIRoot.ELayerType layerType, bool isPooling = true)
	{
		_prefabAuto = new PrefabAuto<T>(path, isGlobal: true, EPrefabType.Ngui, isPooling, touchMaterial: false);
		_layerType = layerType;
	}

	public void ReserveCache(UiPreloader preloader)
	{
		_prefabAuto.ReservePrefab();
		if (_prefabAuto.Value != null)
		{
			T component = _prefabAuto.Value.LoadGo.GetComponent<T>();
			if (component != null)
			{
				component.ReserveCache(preloader);
			}
		}
	}

	public void CreateInstance()
	{
		if (!(_instance != null) && _prefabAuto.Factory != null)
		{
			LayerInfo layerInfo = GlobalUIRoot.instance.GetLayerInfo(_layerType);
			_instance = _prefabAuto.Factory.Create(GlobalUIRoot.instance.ContentsGroup.transform, layerInfo.Camera.gameObject.layer);
		}
	}

	public bool IsOpen()
	{
		if (_instance == null)
		{
			return false;
		}
		return _instance.IsOpen();
	}

	public void CloseWindow()
	{
		if (!(_instance == null))
		{
			_instance.Close();
		}
	}

	public void ReleaseWindow()
	{
		if (!(_instance == null))
		{
			if (_instance.IsOpen())
			{
				_instance.Close();
			}
			FactoryBase.SafeReleaseComponent(ref _instance);
		}
	}

	public void ReleasePrefab()
	{
		_prefabAuto.ReleasePrefab();
	}
}
