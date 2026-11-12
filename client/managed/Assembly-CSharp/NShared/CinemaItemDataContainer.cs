namespace NShared;

public class CinemaItemDataContainer : SubItemDataContainer<CinemaItemData>
{
	public const string TABLENAME = "CinemaItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CinemaItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.CinemaPackage };
	}
}
