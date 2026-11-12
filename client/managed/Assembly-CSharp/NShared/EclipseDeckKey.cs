namespace NShared;

public class EclipseDeckKey : Tuple<int, int>
{
	public EclipseDeckKey()
		: base(0, 0)
	{
	}

	public EclipseDeckKey(int deckIndex, int heroIndex)
		: base(deckIndex, heroIndex)
	{
	}
}
