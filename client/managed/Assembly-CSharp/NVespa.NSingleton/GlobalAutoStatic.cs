using UnityEngine;

namespace NVespa.NSingleton;

public class GlobalAutoStatic<T> where T : MonoBehaviour
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
			GameObject singletonComponent = GlobalAutoManager.SingletonComponent;
			if (!(singletonComponent == null))
			{
				GameObject gameObject = new GameObject();
				gameObject.name = typeof(T).FullName;
				gameObject.transform.parent = singletonComponent.transform;
				AssignInstance(Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<T>(gameObject));
			}
		}
	}

	public static void ReleaseInstance()
	{
		if (!(_instance == null))
		{
			VespaObject.DestroyGo(_instance);
			_instance = null;
		}
	}

	internal static void AssignInstance(T i_instance)
	{
		if (!(_instance != null))
		{
			_instance = i_instance;
			if (_instance is InitOnCreate initOnCreate)
			{
				initOnCreate.InitializeSingleton();
			}
		}
	}
}
