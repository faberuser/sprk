using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class MatchTierDataContainer : JitDataContainerDefault<int, MatchTierData>
{
	public const string TABLENAME = "MatchTierTable";

	private List<MatchTierType> _tierList;

	public override TablePathDefine PathDefine => new TablePathDefine("MatchTierTable");

	public List<MatchTierType> GetAllTierTypeList()
	{
		if (_tierList != null)
		{
			return _tierList;
		}
		_tierList = new List<MatchTierType>();
		foreach (MatchTierData allDatum in GetAllData())
		{
			if (!_tierList.Contains(allDatum.Tier))
			{
				_tierList.Add(allDatum.Tier);
			}
		}
		_tierList.Sort();
		return _tierList;
	}

	public void GetTierLevel(int matchScore, out MatchTierType tierType, out int tierLevel)
	{
		tierType = MatchTierType.Bronze;
		tierLevel = 1;
		foreach (MatchTierData allDatum in GetAllData())
		{
			if (matchScore >= allDatum.MinScore && matchScore <= allDatum.MaxScore)
			{
				tierType = allDatum.Tier;
				tierLevel = allDatum.Level;
				break;
			}
		}
	}

	public List<MatchTierData> GetTierDataListOfType(params MatchTierType[] tierTypes)
	{
		List<MatchTierData> list = new List<MatchTierData>();
		foreach (MatchTierData allDatum in GetAllData())
		{
			if (tierTypes.Contains(allDatum.Tier))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
