namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroData : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => HeroIndex;

	public int HeroIndex { get; set; }

	public int TagType { get; set; }

	public int[] TranscendSkillIndices { get; set; }

	public string EquipSetItem { get; set; }

	public string EquipItemOption { get; set; }

	public int EquipTreasureItemIndex { get; set; }
}
