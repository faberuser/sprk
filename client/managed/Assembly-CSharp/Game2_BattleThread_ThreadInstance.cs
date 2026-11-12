using NGame2.NBattleThread;

public static class Game2_BattleThread_ThreadInstance
{
	public static bool ISVALID(this ThreadInstance This)
	{
		if (This != null)
		{
			return This._battleInstance != null;
		}
		return false;
	}
}
