using NShared.NJit;

namespace NShared;

public class CreatureStarPriceDataContainer : JitDataContainerDefault<CreatureStarPriceKey, CreatureStarPriceData>
{
	public const string TABLENAME = "CreatureStarPriceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureStarPriceTable");

	public CreatureStarPriceData GetData(CreatureOpenStatusType OpenStatus, int Star)
	{
		return GetData(new CreatureStarPriceKey(OpenStatus, Star));
	}
}
