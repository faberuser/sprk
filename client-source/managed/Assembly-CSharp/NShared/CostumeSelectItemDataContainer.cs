namespace NShared;

public class CostumeSelectItemDataContainer : SubItemDataContainer<CostumeSelectItemData>
{
	public const string TABLENAME = "CostumeSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CostumeSelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.CostumeSelectItem };
	}
}
