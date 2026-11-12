using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PunishmentRaidDataContainer : JitDataContainerDefault<RaidDataKey2, PunishmentRaidData>
{
	public const string TABLENAME = "PunishmentRaidTable";

	private JitDictionary<int, List<PunishmentRaidData>> _dicByGroupIndex = new JitDictionary<int, List<PunishmentRaidData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRaidTable");

	public PunishmentRaidDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByGroupIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PunishmentRaidData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByGroupIndex.JsonLoad_AddToListValue(item.GroupIndex, item);
		}
	}

	public IList<PunishmentRaidData> GetPunishmentRaidDatas(int groupIndex)
	{
		return _dicByGroupIndex.GetOrDefault(groupIndex);
	}

	public PunishmentRaidData GetData(int index, int level)
	{
		return GetData(new RaidDataKey2(index, level));
	}
}
