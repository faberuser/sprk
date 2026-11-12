namespace NVespa.NSingleton;

public class SceneBehaviour<T> : BaseSceneBehaviour where T : SceneBehaviour<T>
{
	private static T _instance;

	public static bool isValidInstance => _instance != null;

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				VespaDebug.LogError($"{typeof(T).Name} SingletonBehaviour is null");
			}
			return _instance;
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

	public override void ForceAwake()
	{
		_instance = this as T;
	}
}
