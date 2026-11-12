using UnityEngine;

namespace NVespa.NSingleton;

public static class SceneAutoManager
{
	public const string GAMEOBJECTNAME = "<Scene.Singleton>";

	private static GameObject _singletonComponent;

	private static EmptyBehaviour _singletonBehaviour;

	public static MonoBehaviour SingletonBehaviour => _singletonBehaviour;

	public static GameObject SingletonComponent
	{
		get
		{
			if (_singletonComponent != null)
			{
				return _singletonComponent;
			}
			_singletonComponent = GetGlobalGameObject("<Scene.Singleton>");
			if (_singletonComponent != null)
			{
				_singletonBehaviour = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<EmptyBehaviour>(_singletonComponent);
				_singletonBehaviour.OnDestroyList.Add(delegate
				{
					_singletonBehaviour = null;
					_singletonComponent = null;
				});
			}
			return _singletonComponent;
		}
	}

	private static GameObject GetGlobalGameObject(string i_name)
	{
		if (!SystemStatus.IsPlayingCritical)
		{
			VespaDebug.LogError("플레이중이 아닐때는 Global Component에 접근 금지");
			return null;
		}
		GameObject gameObject = GameObject.Find(i_name);
		if (gameObject == null)
		{
			gameObject = new GameObject(i_name);
		}
		if (!gameObject.activeInHierarchy)
		{
			gameObject.SetActive(value: true);
		}
		return gameObject;
	}
}
