using System.Collections.Generic;
using NShared;

namespace NGame2.NUtil.NDataHelper;

public class BanRuleDataHelper
{
	public static List<CreatureData> ConvBanListStringToIntList(string heroBanList)
	{
		string[] array = heroBanList.Split(',');
		List<CreatureData> list = new List<CreatureData>(__Extension_Method_ICollection__.COUNT(array));
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(array); i < num; i++)
		{
			string code = array[i];
			CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(code);
			if (dataByCode != null)
			{
				list.Add(dataByCode);
			}
		}
		return list;
	}
}
