using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GuildSkillDataContainer : JitDataContainerDefault<int, GuildSkillData>
{
	public const string TABLENAME = "GuildSkillTable";

	private JitRef<List<int>> _guildSkillIndices = new JitRef<List<int>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSkillTable");

	public GuildSkillDataContainer()
	{
		base.Packer.RegisterAndBind(_guildSkillIndices);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GuildSkillData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!_guildSkillIndices.JsonLoad_GetOrNew().Contains(item.GuildSkillIndex))
			{
				_guildSkillIndices.JsonLoad_GetOrNew().Add(item.GuildSkillIndex);
			}
		}
	}

	public List<int> GetGuildSkillIndexListByType(GuildSkillType guildSkillType)
	{
		IList<GuildSkillData> allData = GetAllData();
		List<int> list = new List<int>();
		for (int i = 0; i < allData.Count; i++)
		{
			if (guildSkillType == allData[i].GuildSkillType)
			{
				list.Add(allData[i].GuildSkillIndex);
			}
		}
		return list;
	}

	public int[] GetAllGuildSkillIndices()
	{
		return _guildSkillIndices.Eval().ToArray();
	}

	public List<GuildSkillData> GetGuildSkillsByCreatureTagType(CreatureTagType creatureTagType)
	{
		List<GuildSkillData> list = new List<GuildSkillData>();
		foreach (GuildSkillData allDatum in GetAllData())
		{
			GuildSkillLevelData data = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(new GuildSkillLevelKey2(allDatum.GuildSkillIndex, 1));
			if (data != null)
			{
				SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(data.SkillIndex);
				if (data2 != null && data2.CreatureTagType == creatureTagType)
				{
					list.Add(allDatum);
				}
			}
		}
		return list;
	}
}
