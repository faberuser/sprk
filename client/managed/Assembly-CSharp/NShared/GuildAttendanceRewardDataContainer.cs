using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GuildAttendanceRewardDataContainer : JitDataContainerDefault<GuildAttendanceRewardDataKey, GuildAttendanceRewardData>
{
	public const string TABLENAME = "GuildAttendanceRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildAttendanceRewardTable");

	public List<GuildAttendanceRewardData> GetList(EGuildAttendanceType type)
	{
		IList<GuildAttendanceRewardData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<GuildAttendanceRewardData> list = new List<GuildAttendanceRewardData>();
		list.Clear();
		for (int i = 0; i < allData.Count; i++)
		{
			GuildAttendanceRewardData guildAttendanceRewardData = allData[i];
			if (guildAttendanceRewardData != null && guildAttendanceRewardData.Index == (int)type)
			{
				list.Add(guildAttendanceRewardData);
			}
		}
		return list;
	}

	public GuildAttendanceRewardData GetPerfectAttendanceRewardDataByStep(int step)
	{
		List<GuildAttendanceRewardData> list = GetList(EGuildAttendanceType.Perfect);
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			GuildAttendanceRewardData guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData != null && guildAttendanceRewardData.Step == step + 1)
			{
				return guildAttendanceRewardData;
			}
		}
		return null;
	}

	public GuildAttendanceRewardData GetDataPerfectAttendanceLastStep()
	{
		int num = -1;
		List<GuildAttendanceRewardData> list = GetList(EGuildAttendanceType.Perfect);
		if (list == null)
		{
			return null;
		}
		GuildAttendanceRewardData result = null;
		for (int i = 0; i < list.Count; i++)
		{
			GuildAttendanceRewardData guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData != null && guildAttendanceRewardData.Step > num)
			{
				num = guildAttendanceRewardData.Step;
				result = guildAttendanceRewardData;
			}
		}
		return result;
	}
}
