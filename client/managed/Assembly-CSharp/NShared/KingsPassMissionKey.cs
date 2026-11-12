namespace NShared;

public class KingsPassMissionKey : Tuple<int, int>
{
	public KingsPassMissionKey()
		: base(0, 0)
	{
	}

	public KingsPassMissionKey(int seasonIndex, int missionIndex)
		: base(seasonIndex, missionIndex)
	{
	}
}
