using NGameAsset;
using UnityEngine;

namespace NVespa.NPrefab2.NAsyncLoad;

public class ComponentLoad<T> : AsyncLoader.IPrefabLoad, IAsyncLoadBase where T : Component
{
	private PrefabKey _key;

	private float _beginLoad;

	private float _endLoad;

	private ResourceRequest _resourceRequest;

	private bool _isDone;

	private bool _isValid;

	PrefabKey AsyncLoader.IPrefabLoad.Key => _key;

	float AsyncLoader.IPrefabLoad.BeginLoad => _beginLoad;

	float AsyncLoader.IPrefabLoad.EndLoad => _endLoad;

	string IAsyncLoadBase.KeyDesc => _key.Path;

	bool IAsyncLoadBase.IsDone => _isDone;

	bool IAsyncLoadBase.IsValid => _isValid;

	string IAsyncLoadBase.LoadMessage => string.Empty;

	public void Open(PrefabKey key)
	{
		_key = key;
	}

	void AsyncLoader.IPrefabLoad.StartLoad()
	{
		_beginLoad = Time.realtimeSinceStartup;
		if (SingletonAuto<Container>.instance.Contains<T>(_key))
		{
			Close(isValid: true);
			return;
		}
		_resourceRequest = VespaResource.LoadAsync<GameObject>(_key.Path);
		if (_resourceRequest == null)
		{
			Close(isValid: false);
		}
		else
		{
			SingletonUpdater<AsyncManager>.instance.Add(_resourceRequest, OnComplete);
		}
	}

	private void OnComplete()
	{
		if (_resourceRequest == null)
		{
			Close(isValid: false);
			return;
		}
		GameObject gameObject = _resourceRequest.asset as GameObject;
		if (gameObject == null)
		{
			Close(isValid: false);
			return;
		}
		SingletonAuto<Container>.instance.Register<T>(gameObject, _key);
		Close(isValid: true);
	}

	private void Close(bool isValid)
	{
		_endLoad = Time.realtimeSinceStartup;
		_isDone = true;
		_isValid = isValid;
		_resourceRequest = null;
	}
}
