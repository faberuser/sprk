using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public abstract class BattleViewGenericSingleton<T> : MonoBehaviour where T : BattleViewGenericSingleton<T>
{
	private static T _instance;

	public static bool IsValidInstance => _instance != null;

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
			GameObject obj = new GameObject("BattleView." + typeof(T).Name)
			{
				layer = Game3DRoot.instance.GameCamera.GetLayerInfo(Game3DRoot.ELayerType.Default).Layer
			};
			Vespa_Unity_Transform_Extension_Method.ParentResetTM(obj.transform, Game3DRoot.instance.ContentsGroup.transform);
			_instance = obj.AddComponent<T>();
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
