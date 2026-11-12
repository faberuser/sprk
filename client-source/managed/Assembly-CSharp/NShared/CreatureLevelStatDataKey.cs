namespace NShared;

public class CreatureLevelStatDataKey : Tuple<int, short>
{
	public int CreatureIndex => Item1;

	public int Level => Item2;

	public CreatureLevelStatDataKey()
		: base(0, (short)0)
	{
	}

	public CreatureLevelStatDataKey(int creatureIndex, int level)
		: base(creatureIndex, (short)level)
	{
	}
}
