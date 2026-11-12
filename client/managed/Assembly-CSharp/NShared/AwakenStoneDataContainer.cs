namespace NShared;

public class AwakenStoneDataContainer : SubItemDataContainer<AwakenStoneData>
{
	public const string TABLENAME = "AwakenStoneTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AwakenStoneTable");

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[2]
		{
			ItemType.PublicUniqueEquipAwaken,
			ItemType.PublicTreasureAwaken
		};
	}
}
