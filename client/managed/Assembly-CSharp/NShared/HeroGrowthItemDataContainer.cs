namespace NShared;

public class HeroGrowthItemDataContainer : SubItemDataContainer<HeroGrowthItemData>
{
	public const string TABLENAME = "HeroGrowthItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroGrowthItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.HeroGrowth };
	}
}
