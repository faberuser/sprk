using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class DOWDungeonTextDataContainer : JitDataContainerDefault<DOWDungeonTextKey, DOWDungeonTextData>
{
	public const string TABLENAME = "DOWDungeonTextTable";

	private const int MaxDiff = 4;

	public override TablePathDefine PathDefine => new TablePathDefine("DOWDungeonTextTable");

	public DOWDungeonTextData GetData(int DungeonIndex, int Diff)
	{
		return GetData(new DOWDungeonTextKey(DungeonIndex, Diff));
	}

	public List<DOWDungeonTextData> GetDatas(int DungeonIndex)
	{
		List<DOWDungeonTextData> list = null;
		foreach (DOWDungeonTextData allDatum in GetAllData())
		{
			if (allDatum.DungeonIndex == DungeonIndex && allDatum.Diff <= 4)
			{
				if (list == null)
				{
					list = new List<DOWDungeonTextData>();
				}
				list.Add(allDatum);
			}
		}
		return list;
	}
}
