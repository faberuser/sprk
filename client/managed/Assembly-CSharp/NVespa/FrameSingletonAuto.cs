namespace NVespa;

public class FrameSingletonAuto<T> : IFrameSingleton where T : class, IFrameSingleton, new()
{
	public static bool isValidInstance => FrameSingletonCollection.IsValidInstance<T>();

	public static T instance => FrameSingletonCollection.GetInstance<T>();

	public static void ReleaseInstance()
	{
		FrameSingletonCollection.ReleaseInstance<T>();
	}
}
