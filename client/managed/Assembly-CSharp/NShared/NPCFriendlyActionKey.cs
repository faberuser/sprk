namespace NShared;

[JsonSerializable]
public class NPCFriendlyActionKey : Tuple<int, int>
{
	public NPCFriendlyActionKey()
		: base(0, 0)
	{
	}

	public NPCFriendlyActionKey(int HeroIndex, int ItemGrade)
		: base(HeroIndex, ItemGrade)
	{
	}
}
