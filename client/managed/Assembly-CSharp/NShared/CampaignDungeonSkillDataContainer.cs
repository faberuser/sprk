using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CampaignDungeonSkillDataContainer : JitDataContainerDefault<int, CampaignDungeonSkillData>
{
	public const string TABLENAME = "CampaignDungeonSkillTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignDungeonSkillTable");

	public List<int> GetDungeonSkillIndexListByTeam(int[] indexList, TeamType teamType)
	{
		if (indexList == null || indexList.Length == 0)
		{
			return null;
		}
		List<int> list = new List<int>();
		foreach (int key in indexList)
		{
			CampaignDungeonSkillData data = GetData(key);
			if (data != null && data.TeamType == teamType)
			{
				list.Add(data.BaseSkillIndex);
			}
		}
		return list;
	}
}
