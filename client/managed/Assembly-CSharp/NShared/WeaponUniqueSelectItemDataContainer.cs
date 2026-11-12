namespace NShared;

public class WeaponUniqueSelectItemDataContainer : SubItemDataContainer<WeaponUniqueSelectItemData>
{
	public const string TABLENAME = "WeaponUniqueSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WeaponUniqueSelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.WeaponUniqueSelectItem };
	}
}
