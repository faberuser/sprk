namespace NShared;

[JsonSerializable(Javascript = true)]
public class DeckInfo : IAccountAttrKey<BattleType>
{
	BattleType IAccountAttrKey<BattleType>.Key => BattleType;

	public BattleType BattleType { get; set; }

	public int HeroIndex1 { get; set; }

	public int HeroIndex2 { get; set; }

	public int HeroIndex3 { get; set; }

	public int HeroIndex4 { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public string PlayedTime { get; set; }

	public int GetHeroIndex(int idx)
	{
		return idx switch
		{
			0 => HeroIndex1, 
			1 => HeroIndex2, 
			2 => HeroIndex3, 
			3 => HeroIndex4, 
			_ => 0, 
		};
	}
}
