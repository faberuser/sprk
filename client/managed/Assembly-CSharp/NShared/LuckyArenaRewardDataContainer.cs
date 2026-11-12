using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class LuckyArenaRewardDataContainer : JitDataContainerDefault<LuckyArenaRewardDataKey, LuckyArenaRewardData>
{
	public const string TABLENAME = "LuckyArenaRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("LuckyArenaRewardTable");

	public LuckyArenaRewardData GetData(LuckyMatchRewardType type, int showIndex = 0)
	{
		return GetData(new LuckyArenaRewardDataKey(type, showIndex));
	}

	public List<LuckyArenaRewardData> GetAllData(LuckyMatchRewardType type)
	{
		List<LuckyArenaRewardData> list = new List<LuckyArenaRewardData>();
		int num = 1;
		while (true)
		{
			LuckyArenaRewardData data = GetData(type, num);
			if (data == null)
			{
				break;
			}
			list.Add(data);
			num++;
		}
		return list;
	}
}
