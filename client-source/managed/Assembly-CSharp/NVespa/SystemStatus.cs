namespace NVespa;

public static class SystemStatus
{
	public enum EMemorySpec
	{
		Low,
		Mid,
		High
	}

	public static bool IsApplicationQuit => UnityCache.isQuiting;

	public static bool IsPlaying => UnityCache.isGamePlaying;

	public static bool IsPlayingCritical => UnityCache.isGamePlaying;

	static SystemStatus()
	{
	}

	public static void OnApplicationQuit()
	{
	}
}
