using System;
using System.Collections.Generic;
using MessagePack;
using NVespa;

namespace NShared;

[MessagePackObject(false)]
public class SkillProjectileGroupData : IDataKeySelector<int>
{
	public class SkillProjectileInfo
	{
		public int Index;

		public TeamType Team;
	}

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string[] SkillProjectileGroupIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}

	public List<SkillProjectileInfo> GetSkillProjectileInfos()
	{
		List<SkillProjectileInfo> list = new List<SkillProjectileInfo>();
		if (SkillProjectileGroupIndex != null)
		{
			string[] skillProjectileGroupIndex = SkillProjectileGroupIndex;
			for (int i = 0; i < skillProjectileGroupIndex.Length / 2; i++)
			{
				try
				{
					string s = skillProjectileGroupIndex[i * 2].Trim();
					string value = skillProjectileGroupIndex[i * 2 + 1].Trim();
					int index = int.Parse(s);
					TeamType team = (TeamType)Enum.Parse(typeof(TeamType), value);
					list.Add(new SkillProjectileInfo
					{
						Index = index,
						Team = team
					});
				}
				catch (Exception ex)
				{
					VespaDebug.LogError("Error while creating random projectile. error=" + ex);
					return null;
				}
			}
		}
		return list;
	}
}
