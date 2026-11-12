namespace NShared;

public class PackageItemDataContainer : SubItemDataContainer<PackageItemData>
{
	public const string TABLENAME = "PackageItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PackageItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Package };
	}
}
