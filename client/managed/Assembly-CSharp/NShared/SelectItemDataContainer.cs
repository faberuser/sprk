namespace NShared;

public class SelectItemDataContainer : SubItemDataContainer<SelectItemData>
{
	public const string TABLENAME = "SelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.PackageSelectItem };
	}
}
