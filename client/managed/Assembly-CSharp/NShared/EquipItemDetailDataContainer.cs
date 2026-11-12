using NShared.NJit;

namespace NShared;

public class EquipItemDetailDataContainer : JitDataContainerDefault<int, EquipItemDetailData>
{
	public const string TABLENAME = "EquipItemDetailTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipItemDetailTable");

	public int GetEquipItemMaxLevel(int detailIndex, int heroLevel)
	{
		EquipItemDetailData data = GetData(detailIndex);
		if (data == null)
		{
			return 0;
		}
		if (data.MaxLevelByHero)
		{
			return heroLevel;
		}
		return data.MaxLevel;
	}

	public int GetRandomOptionCount(int detailIndex)
	{
		return GetData(detailIndex)?.OptionCount ?? 0;
	}

	public int GetRuneSlotCount(int detailIndex)
	{
		return 0;
	}
}
