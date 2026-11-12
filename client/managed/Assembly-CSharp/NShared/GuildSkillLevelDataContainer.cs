using System;
using NShared.NJit;

namespace NShared;

public class GuildSkillLevelDataContainer : JitDataContainerDefault<GuildSkillLevelKey2, GuildSkillLevelData>
{
	public const string TABLENAME = "GuildSkillLevelTable";

	private JitDictionary<int, int> _maxLevels_by_SkillIndex = new JitDictionary<int, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSkillLevelTable");

	public GuildSkillLevelDataContainer()
	{
		base.Packer.RegisterAndBind(_maxLevels_by_SkillIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GuildSkillLevelData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (!_maxLevels_by_SkillIndex.JsonLoad_Get().ContainsKey(item.GuildSkillIndex))
			{
				_maxLevels_by_SkillIndex.JsonLoad_Get().Add(item.GuildSkillIndex, item.GuildSkillLevel);
				continue;
			}
			int val = _maxLevels_by_SkillIndex.JsonLoad_Get()[item.GuildSkillIndex];
			_maxLevels_by_SkillIndex.JsonLoad_Get()[item.GuildSkillIndex] = Math.Max(val, item.GuildSkillLevel);
		}
	}

	public GuildSkillLevelData GetData(int GuildSkillIndex, int GuildSkillLevel)
	{
		return GetData(new GuildSkillLevelKey2(GuildSkillIndex, GuildSkillLevel));
	}

	public int GetMaxSkillLevel(int GuildSkillIndex)
	{
		return _maxLevels_by_SkillIndex.GetOrDefault(GuildSkillIndex);
	}
}
