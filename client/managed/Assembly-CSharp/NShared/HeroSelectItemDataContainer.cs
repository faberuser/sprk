namespace NShared;

public class HeroSelectItemDataContainer : SubItemDataContainer<HeroSelectItemData>
{
	public const string TABLENAME = "HeroSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroSelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.HeroSelect };
	}
}
