using System.Collections;
using NShared.NJit;

namespace NShared;

public class EquipItemBreakDataContainer : JitDataContainerDefault<EquipItemBreakDataKey, EquipItemBreakData>
{
	public const string TABLENAME = "EquipItemBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipItemBreakTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, EquipItemBreakData value)
	{
		if (String_Extension_Method.IsNullOrEmpty(value.ItemCode))
		{
			value.ItemCode = string.Empty;
			return;
		}
		value.Tier = 0;
		value.DetailIndex = 0;
		value.Star = 0;
	}

	public EquipItemBreakData GetData(int tier, int detailIndex, string itemCode, int star = 0, int setIndex = 0)
	{
		if (setIndex > 0)
		{
			return GetData(new EquipItemBreakDataKey(tier, detailIndex, string.Empty, 0, setIndex));
		}
		EquipItemBreakData data = GetData(new EquipItemBreakDataKey(0, 0, itemCode, 0, 0));
		if (data != null)
		{
			return data;
		}
		for (int num = star; num >= 0; num--)
		{
			data = GetData(new EquipItemBreakDataKey(tier, detailIndex, string.Empty, num, 0));
			if (data != null)
			{
				return data;
			}
		}
		return data;
	}
}
