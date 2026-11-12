namespace NShared;

public class HeroLimitBreakExpItemDataContainer : SubItemDataContainer<HeroLimitBreakExpItemData>
{
	public const string TABLENAME = "HeroLimitBreakExpItemTable";

	public override TablePathDefine PathDefine => new TablePathDefine("HeroLimitBreakExpItemTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.HeroLimitBreakExp };
	}
}
