using NShared.NJit;

namespace NShared;

public class CreatureStarStatDataContainer : JitDataContainerDefault<CreatureStarStatDataKey, CreatureStarStatData>
{
	public const string TABLENAME = "CreatureStarStatTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureStarStatTable");

	public bool GetStatFactor(int star, int transcended, StatType statType, out int factor)
	{
		CreatureStarStatData data = GetData(new CreatureStarStatDataKey(star, transcended, statType));
		if (data == null)
		{
			factor = 1000;
			return false;
		}
		factor = data.StatFactor;
		return true;
	}
}
