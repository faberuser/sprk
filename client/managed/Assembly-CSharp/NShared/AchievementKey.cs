namespace NShared;

public class AchievementKey : Tuple<int, int>
{
	public AchievementKey()
		: base(0, 0)
	{
	}

	public AchievementKey(int Index, int Step)
		: base(Index, Step)
	{
	}
}
