namespace NShared;

public class PetExploreInfoKey : Tuple<int, int>
{
	public int ExploreIndex
	{
		get
		{
			return Item1;
		}
		set
		{
			Item1 = value;
		}
	}

	public int DeckIndex
	{
		get
		{
			return Item2;
		}
		set
		{
			Item2 = value;
		}
	}

	public PetExploreInfoKey(int exploreIndex, int deckIndex)
		: base(exploreIndex, deckIndex)
	{
	}
}
