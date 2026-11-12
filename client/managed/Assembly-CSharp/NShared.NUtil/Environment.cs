namespace NShared.NUtil;

public class Environment
{
	public static bool IsClient()
	{
		return true;
	}

	public static bool IsClientEditor()
	{
		return false;
	}

	public static bool IsServer()
	{
		return !IsClient();
	}

	public static bool NeedTest()
	{
		if (!IsServer())
		{
			return IsClientEditor();
		}
		return true;
	}
}
