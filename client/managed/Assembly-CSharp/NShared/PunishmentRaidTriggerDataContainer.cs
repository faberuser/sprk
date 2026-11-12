using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PunishmentRaidTriggerDataContainer : JitDataContainerDefault<int, PunishmentRaidTriggerData>
{
	public const string TABLENAME = "PunishmentRaidTriggerTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRaidTriggerTable");

	public List<PunishmentRaidTriggerData> GetPunishmentRaidTriggerDatas(int[] triggerIndices)
	{
		List<PunishmentRaidTriggerData> list = new List<PunishmentRaidTriggerData>();
		for (int i = 0; i < triggerIndices.Length; i++)
		{
			list.Add(GetData(triggerIndices[i]));
		}
		return list;
	}
}
