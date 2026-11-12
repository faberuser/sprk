namespace NGame2.NContentsDefine;

public class ItemRelation
{
	public struct HeroRelation
	{
		public int HeroIndex;

		public int UniqueWeaponItemIndex;

		public int SoulStoneItemIndex;
	}

	private const int _add_for_HeroIndex_to_UniqueWeaponItemIndex = 1000;

	private const int _add_for_HeroIndex_to_SoulStoneItemIndex = 111000;

	public static HeroRelation GetHeroRelation_by_HeroIndex(int heroIndex)
	{
		return new HeroRelation
		{
			HeroIndex = heroIndex,
			UniqueWeaponItemIndex = heroIndex + 1000,
			SoulStoneItemIndex = heroIndex + 111000
		};
	}

	public static HeroRelation GetHeroRelation_by_UniqueWeaponItemIndex(int itemIndex)
	{
		return GetHeroRelation_by_HeroIndex(itemIndex - 1000);
	}

	public static HeroRelation GetHeroRelation_by_SoulStoneItemIndex(int itemIndex)
	{
		return GetHeroRelation_by_HeroIndex(itemIndex - 111000);
	}
}
