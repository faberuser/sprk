using System;

namespace NVespa;

public class Singleton<TypeCls> where TypeCls : class
{
	private static TypeCls _instance;

	public static TypeCls instance
	{
		get
		{
			if (_instance == null)
			{
				Type typeFromHandle = typeof(TypeCls);
				VespaDebug.LogError($"{typeFromHandle.Name} Singleton is null");
			}
			return _instance;
		}
	}

	protected Singleton()
	{
		if (_instance != null)
		{
			VespaDebug.LogError("create singleton twice: " + _instance);
		}
		else
		{
			_instance = this as TypeCls;
		}
	}
}
