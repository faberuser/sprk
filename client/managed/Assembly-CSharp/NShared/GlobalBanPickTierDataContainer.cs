using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class GlobalBanPickTierDataContainer : JitDataContainerDefault<int, MatchBanPickTierData>
{
	public const string TABLENAME = "GlobalBanPickTierTable";

	private List<MatchTierType> _tierList;

	public override TablePathDefine PathDefine => new TablePathDefine("GlobalBanPickTierTable");

	public IList<MatchTierType> GetAllTierTypeList()
	{
		if (_tierList != null)
		{
			return _tierList;
		}
		_tierList = new List<MatchTierType>();
		foreach (MatchBanPickTierData allDatum in GetAllData())
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
		foreach (MatchBanPickTierData allDatum in GetAllData())
		{
			if (matchScore >= allDatum.MinScore && matchScore <= allDatum.MaxScore)
			{
				tierType = allDatum.Tier;
				tierLevel = allDatum.Level;
				break;
			}
		}
	}

	public List<MatchBanPickTierData> GetTierDataListOfType(params MatchTierType[] tierTypes)
	{
		List<MatchBanPickTierData> list = new List<MatchBanPickTierData>();
		foreach (MatchBanPickTierData allDatum in GetAllData())
		{
			if (tierTypes.Contains(allDatum.Tier))
			{
				list.Add(allDatum);
			}
		}
		return list;
	}
}
