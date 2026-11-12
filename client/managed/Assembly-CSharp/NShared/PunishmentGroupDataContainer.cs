using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PunishmentGroupDataContainer : JitDataContainerDefault<PunishmentGroupDataKey, PunishmentGroupData>
{
	public const string TABLENAME = "PunishmentGroupTable";

	private JitRef<List<PunishmentGroupData>> _list_of_Boss = new JitRef<List<PunishmentGroupData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentGroupTable");

	public PunishmentGroupDataContainer()
	{
		base.Packer.RegisterAndBind(_list_of_Boss);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PunishmentGroupData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.PunishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				_list_of_Boss.JsonLoad_GetOrNew().Add(item);
			}
		}
		_list_of_Boss.JsonLoad_GetOrNew().Sort((PunishmentGroupData lhs, PunishmentGroupData rhs) => lhs.GroupIndex.CompareTo(rhs.GroupIndex));
	}

	public PunishmentGroupData GetData(int index, EPunishmentDungeonType punishmentDungeonType)
	{
		return GetData(new PunishmentGroupDataKey(index, punishmentDungeonType));
	}

	public PunishmentGroupData GetKarmaBossPunishmentData(int karmaRaidIndex)
	{
		foreach (PunishmentGroupData allDatum in GetAllData())
		{
			if (allDatum.KarmaRaidIndex == karmaRaidIndex)
			{
				return allDatum;
			}
		}
		return null;
	}

	public IList<PunishmentGroupData> GetBossPunishmentDataList()
	{
		return _list_of_Boss.Eval();
	}

	public PunishmentGroupData GetDataWithChapterIndex(int chapterIndex)
	{
		foreach (PunishmentGroupData allDatum in GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex)
			{
				return allDatum;
			}
		}
		return null;
	}
}
