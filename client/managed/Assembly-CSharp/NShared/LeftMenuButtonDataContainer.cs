using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class LeftMenuButtonDataContainer : JitDataContainerDefault<int, LeftMenuButtonData>
{
	public const string TABLENAME = "LeftMenuButtonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("LeftMenuButtonTable");

	public List<LeftMenuButtonData> SortedList { get; private set; }

	public List<LeftMenuButtonData> GetSortedList()
	{
		if (SortedList == null)
		{
			SortedList = new List<LeftMenuButtonData>();
			SortedList.AddRange(GetAllData());
			SortedList.Sort((LeftMenuButtonData l, LeftMenuButtonData r) => l.ShowIndex.CompareTo(r.ShowIndex));
		}
		return SortedList;
	}
}
