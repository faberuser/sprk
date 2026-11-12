using System.Collections;
using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class SoulBreakDataContainer : JitDataContainerDefault<SoulBreakKey, SoulBreakData>
{
	public const string TABLENAME = "SoulBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulBreakTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, SoulBreakData value)
	{
		value.ItemMinMaxList = new List<SoulBreakData.ItemCodeMinMaxData>();
		for (int i = 1; i <= 5; i++)
		{
			if (jsonDic.Contains("ItemCode" + i))
			{
				SoulBreakData.ItemCodeMinMaxData itemCodeMinMaxData = JsonMarshalerUsingReflection.Parse<SoulBreakData.ItemCodeMinMaxData>(jsonDic, string.Empty, i.ToString());
				if (itemCodeMinMaxData != null && !String_Extension_Method.IsNullOrEmpty(itemCodeMinMaxData.ItemCode))
				{
					value.ItemMinMaxList.Add(itemCodeMinMaxData);
				}
			}
		}
	}

	public SoulBreakData GetData(CreatureTagType type, int grade, int reinforceLevel)
	{
		return GetData(new SoulBreakKey(type, grade, reinforceLevel));
	}
}
