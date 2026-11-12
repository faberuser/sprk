using UnityEngine;

namespace NVespa.NSingleton;

public class SceneAutoBehaviour<T> : MonoBehaviour where T : SceneAutoBehaviour<T>
{
	internal static T _instance;

	public static bool isValidInstance => !Vespa_Common_Class_Extension_Method.IsUnityNull(_instance);

	public static T instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance()
	{
		if (!(_instance != null))
		{
			_instance = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<T>(SceneAutoManager.SingletonComponent);
			if (_instance is InitOnCreate initOnCreate)
			{
				initOnCreate.InitializeSingleton();
			}
		}
	}

	protected virtual void Awake()
	{
		_instance = this as T;
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
