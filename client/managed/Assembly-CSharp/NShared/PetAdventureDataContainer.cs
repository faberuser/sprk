using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PetAdventureDataContainer : JitDataContainerDefault<int, PetAdventureData>
{
	public const string TABLENAME = "PetAdventureTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetAdventureTable");

	public List<PetAdventureData> GetDataListByType(EPetAdventureType Adventure)
	{
		List<PetAdventureData> list = null;
		foreach (PetAdventureData allDatum in GetAllData())
		{
			if (allDatum.AdventureType == Adventure && allDatum.IsShow)
			{
				if (list == null)
				{
					list = new List<PetAdventureData>();
				}
				list.Add(allDatum);
			}
		}
		list?.Sort(delegate(PetAdventureData lhs, PetAdventureData rhs)
		{
			if (lhs == null)
			{
				return 1;
			}
			return (rhs == null) ? (-1) : lhs.Order.CompareTo(rhs.Order);
		});
		return list;
	}
}
