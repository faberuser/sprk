namespace NShared;

public class MultipleHeroSelectItemDataContainer : SubItemDataContainer<MultipleHeroSelectItemData>
{
	public const string TABLENAME = "MultipleHeroSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MultipleHeroSelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.MultipleHeroSelect };
	}
}
