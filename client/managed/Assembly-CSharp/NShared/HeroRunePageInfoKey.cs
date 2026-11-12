namespace NShared;

public class HeroRunePageInfoKey : Tuple<int, int, int>
{
	public HeroRunePageInfoKey(int HeroIndex, int RunePage, int SlotNum)
		: base(HeroIndex, RunePage, SlotNum)
	{
	}
}
