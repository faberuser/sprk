using NShared.NJit;

namespace NShared;

public class TreasureHouseDungeonDataContainer : JitDataContainerDefault<TreasureHouseDungeonKey, TreasureHouseDungeonData>
{
	public const string TABLENAME = "TreasureHouseDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TreasureHouseDungeonTable");

	public TreasureHouseDungeonData GetData(int infoIndex, int showIndex)
	{
		return GetData(new TreasureHouseDungeonKey(infoIndex, showIndex));
	}

	public TreasureHouseDungeonData GetData(int index)
	{
		foreach (TreasureHouseDungeonData allDatum in GetAllData())
		{
			if (allDatum != null && allDatum.Index == index)
			{
				return allDatum;
			}
		}
		return null;
	}
}
