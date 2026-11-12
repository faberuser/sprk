using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class UnderPrisonDataContainer : JitDataContainerDefault<int, UnderPrisonData>
{
	public const string TABLENAME = "UnderPrisonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("UnderPrisonTable");

	public UnderPrisonData GetUnderPrisonDataByUnderPrisonType(UnderPrisonType type)
	{
		IList<UnderPrisonData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		UnderPrisonData underPrisonData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			underPrisonData = allData[i];
			if (underPrisonData != null && underPrisonData.Type == type)
			{
				return underPrisonData;
			}
		}
		return null;
	}
}
