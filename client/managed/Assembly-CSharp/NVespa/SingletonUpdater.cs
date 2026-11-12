using NVespa.NSingleton;

namespace NVespa;

public abstract class SingletonUpdater<TypeCls> : SingletonBase, IUpdater where TypeCls : class, new()
{
	private static TypeCls _instance;

	public static bool isValidInstance => _instance != null;

	public static TypeCls instance
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

	public static void CreateInstance()
	{
		if (_instance == null)
		{
			new TypeCls();
		}
	}

	public static void ReleaseSingleton()
	{
		_instance = null;
	}

	public static TypeCls NewInstance()
	{
		ReleaseSingleton();
		return instance;
	}

	protected SingletonUpdater()
	{
		if (_instance != null)
		{
			VespaDebug.LogError("create singleton twice: " + _instance);
			return;
		}
		_instance = this as TypeCls;
		GlobalUpdateBehaviour.AddUpdater(this);
	}

	void IUpdater.ChainUpdate()
	{
		if ((object)_instance == this)
		{
			ChainUpdate();
		}
	}

	public abstract void ChainUpdate();
}
