namespace NShared;

public class AccessorySelectItemDataContainer : SubItemDataContainer<AccessorySelectItemData>
{
	public const string TABLENAME = "AccessorySelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AccessorySelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.AccessorySelectItem };
	}
}
