using UnityEngine;

namespace NVespa.NSingleton;

public static class GlobalAutoManager
{
	public const string GAMEOBJECTNAME = "<Global.Singleton>";

	private static GameObject _singletonComponent;

	public static bool IsValid => _singletonComponent != null;

	public static GameObject SingletonComponent
	{
		get
		{
			if (_singletonComponent != null)
			{
				return _singletonComponent;
			}
			_singletonComponent = GetGlobalGameObject("<Global.Singleton>");
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
		if (SystemStatus.IsApplicationQuit)
		{
			return null;
		}
		GameObject gameObject = GameObject.Find(i_name);
		if (gameObject == null)
		{
			gameObject = new GameObject(i_name);
			Object.DontDestroyOnLoad(gameObject);
		}
		if (!gameObject.activeInHierarchy)
		{
			gameObject.SetActive(value: true);
		}
		return gameObject;
	}
}
