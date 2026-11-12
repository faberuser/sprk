using UnityEngine;

namespace NVespa.NSingleton;

public class GlobalAutoBehaviour<T> : MonoBehaviour where T : GlobalAutoBehaviour<T>
{
	public static bool isValidInstance => GlobalAutoStatic<T>.isValidInstance;

	public static T instance => GlobalAutoStatic<T>.instance;

	public static T NewInstance()
	{
		GlobalAutoStatic<T>.ReleaseInstance();
		return instance;
	}

	public static void CreateInstance()
	{
		GlobalAutoStatic<T>.CreateInstance();
	}

	protected virtual void Awake()
	{
		GlobalAutoStatic<T>.AssignInstance(this as T);
	}

	protected virtual void OnDestroy()
	{
		if (GlobalAutoStatic<T>._instance == this)
		{
			GlobalAutoStatic<T>._instance = null;
		}
	}

	protected virtual void OnApplicationQuit()
	{
		SystemStatus.OnApplicationQuit();
	}
}
