using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class PetIncubatorSlotDataContainer : JitDataContainerDefault<int, PetIncubatorSlotData>
{
	public const string TABLENAME = "PetIncubatorSlotTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetIncubatorSlotTable");

	public List<PetIncubatorSlotData> GetAllDataByOpenNum()
	{
		List<PetIncubatorSlotData> list = GetAllData().ToList();
		list.Sort(delegate(PetIncubatorSlotData lhs, PetIncubatorSlotData rhs)
		{
			if (lhs == null)
			{
				return 1;
			}
			return (rhs == null) ? (-1) : lhs.OpenNum.CompareTo(rhs.OpenNum);
		});
		return list;
	}
}
