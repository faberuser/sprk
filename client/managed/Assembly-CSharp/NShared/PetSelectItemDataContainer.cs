namespace NShared;

public class PetSelectItemDataContainer : SubItemDataContainer<PetSelectItemData>
{
	public const string TABLENAME = "PetSelectItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetSelectItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.PetSelect };
	}
}
