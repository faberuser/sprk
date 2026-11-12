namespace NShared;

public class EquipEnchantOptionItemDataContainer : SubItemDataContainer<EquipEnchantOptionItemData>
{
	public const string TABLENAME = "EquipEnchantOptionItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipEnchantOptionItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Etc };
	}
}
