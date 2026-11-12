namespace NShared;

public class RouletteRewardKey : Tuple<int, int>
{
	public RouletteRewardKey()
		: base(0, 0)
	{
	}

	public RouletteRewardKey(int Index, int Section)
		: base(Index, Section)
	{
	}
}
