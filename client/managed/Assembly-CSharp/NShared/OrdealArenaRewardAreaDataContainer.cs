using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class OrdealArenaRewardAreaDataContainer : JitDataContainerDefault<OrdealArenaRewardAreaDataKey, OrdealArenaRewardAreaData>
{
	public const string TABLENAME = "OrdealArenaRewardAreaTable";

	private Dictionary<MatchTierType, List<OrdealArenaRewardAreaData>> _dic;

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaRewardAreaTable");

	public OrdealArenaRewardAreaData GetData(int index, MatchTierType tierType)
	{
		return GetData(new OrdealArenaRewardAreaDataKey(index, tierType));
	}

	public Dictionary<MatchTierType, List<OrdealArenaRewardAreaData>> GetAllDataByTierType()
	{
		if (_dic == null)
		{
			_dic = new Dictionary<MatchTierType, List<OrdealArenaRewardAreaData>>();
			foreach (OrdealArenaRewardAreaData allDatum in GetAllData())
			{
				MatchTierType tierType = allDatum.TierType;
				if (_dic.ContainsKey(tierType))
				{
					_dic[tierType].Add(allDatum);
					continue;
				}
				List<OrdealArenaRewardAreaData> list = new List<OrdealArenaRewardAreaData>();
				list.Add(allDatum);
				_dic.Add(tierType, list);
			}
		}
		return _dic;
	}
}
