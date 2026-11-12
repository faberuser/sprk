using System.Text;

namespace NVespa;

public class SingletonAuto<TypeCls> : SingletonBase, ISingleton where TypeCls : class, new()
{
	public static object LockObj = new object();

	private static TypeCls _instance;

	public static bool isValidInstance
	{
		get
		{
			lock (LockObj)
			{
				return _instance != null;
			}
		}
	}

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

	public static TypeCls NewInstance
	{
		get
		{
			lock (LockObj)
			{
				_instance = null;
				return instance;
			}
		}
	}

	public static TypeCls UnsafeInstance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance()
	{
		lock (LockObj)
		{
			if (_instance == null)
			{
				new TypeCls();
				if (_instance == null)
				{
					VespaDebug.LogError("singleton creation fail. it makes null: " + typeof(TypeCls).FullName);
				}
			}
		}
	}

	public static void ReleaseSingleton()
	{
		lock (LockObj)
		{
			_instance = null;
		}
	}

	protected SingletonAuto()
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

	void ISingleton.Dump(StringBuilder builder)
	{
		builder.AppendLine(_instance.GetType().FullName);
	}
}
