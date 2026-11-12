using UnityEngine;

namespace NVespa.NSingleton;

public class GlobalOnceBehaviour<T> : MonoBehaviour where T : GlobalOnceBehaviour<T>
{
	private static bool _created;

	public static bool isValidInstance => GlobalAutoStatic<T>.isValidInstance;

	public static T instance => GlobalAutoStatic<T>.instance;

	public static void CreateInstance()
	{
		if (!_created)
		{
			GlobalAutoStatic<T>.CreateInstance();
			_created = true;
		}
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

	private void OnApplicationQuit()
	{
		SystemStatus.OnApplicationQuit();
	}
}
