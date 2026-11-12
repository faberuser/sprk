namespace NShared;

[JsonSerializable]
public class MyGuildInfo
{
	public long Id { get; set; }

	public string Name { get; set; }

	public byte Level { get; set; }

	public byte Logo { get; set; }

	public byte Back { get; set; }

	public byte Flag { get; set; }

	public string Notice { get; set; }

	public int ReqTeamLevel { get; set; }

	public byte JoinWay { get; set; }

	public long ActivityPoint { get; set; }

	public long SuppressPoint { get; set; }

	public int CurMember { get; set; }

	public string Introduce { get; set; }

	public byte Skill1Level { get; set; }

	public byte Skill2Level { get; set; }

	public byte Skill3Level { get; set; }

	public byte Skill4Level { get; set; }

	public byte Skill5Level { get; set; }

	public byte Skill6Level { get; set; }

	public string SkillInitTime { get; set; }

	public int ContributedGem { get; set; }

	public byte Skill1Req { get; set; }

	public byte Skill2Req { get; set; }

	public byte Skill3Req { get; set; }

	public byte Skill4Req { get; set; }

	public byte Skill5Req { get; set; }

	public byte Skill6Req { get; set; }

	public int Wood { get; set; }

	public int Stone { get; set; }

	public int Metal { get; set; }

	public int AutoApplySuppress { get; set; }

	public string CountryCode { get; set; }

	public GuildBuildingInfo[] GuildBuildingInfos { get; set; }

	public GuildSkillInfo[] SkillInfos { get; set; }

	public void SetGuildBuildingInfo(GuildBuildingInfo guildbuildingInfo)
	{
		RestoredPortal.UpdateGuildBuilding(this, guildbuildingInfo);
	}

	public void SetGuildBuildingInfo(GuildBuildingInfo[] guildbuildingInfos)
	{
		GuildBuildingInfos = guildbuildingInfos;
	}

	public GuildBuildingInfo GetGuildBuildingInfo(EGuildBuildingType buildingType)
	{
		return GetGuildBuildingInfo((int)buildingType);
	}

	public GuildBuildingInfo GetGuildBuildingInfo(int buildingIndex)
	{
		if (GuildBuildingInfos == null)
		{
			return null;
		}
		for (int i = 0; i < GuildBuildingInfos.Length; i++)
		{
			if (GuildBuildingInfos[i].BuildingIndex == buildingIndex)
			{
				return GuildBuildingInfos[i];
			}
		}
		return null;
	}

	public GuildSkillInfo GetGuildSkillInfo(int SkillIndex)
	{
		if (SkillInfos == null)
		{
			return null;
		}
		GuildSkillInfo[] skillInfos = SkillInfos;
		foreach (GuildSkillInfo guildSkillInfo in skillInfos)
		{
			if (guildSkillInfo.SkillIndex == SkillIndex)
			{
				return guildSkillInfo;
			}
		}
		return null;
	}

	public void AddGuildSkillReq(int guildSkillIndex, int add)
	{
		if (SkillInfos == null)
		{
			return;
		}
		GuildSkillInfo[] skillInfos = SkillInfos;
		foreach (GuildSkillInfo guildSkillInfo in skillInfos)
		{
			if (guildSkillInfo.SkillIndex == guildSkillIndex)
			{
				int num = guildSkillInfo.ReqCount + add;
				if (num < 0)
				{
					num = 0;
				}
				else if (num > 255)
				{
					num = 255;
				}
				guildSkillInfo.ReqCount = (byte)num;
			}
		}
	}

	public byte GetSkillLevel(int SkillIndex)
	{
		GuildSkillData data = GameRoot.Instance.GuildSkillDataContainer.GetData(SkillIndex);
		if (data == null)
		{
			return 0;
		}
		return data.GuildSkillIndex switch
		{
			1 => Skill1Level, 
			2 => Skill2Level, 
			3 => Skill3Level, 
			4 => Skill4Level, 
			5 => Skill5Level, 
			6 => Skill6Level, 
			_ => 0, 
		};
	}

	public void ResetSkillLevel()
	{
		if (SkillInfos != null)
		{
			GuildSkillInfo[] skillInfos = SkillInfos;
			for (int i = 0; i < skillInfos.Length; i++)
			{
				skillInfos[i].SkillLevel = 1;
			}
		}
	}

	public void ResetSkillLevelUpReq()
	{
		Skill1Req = 0;
		Skill2Req = 0;
		Skill3Req = 0;
		Skill4Req = 0;
		Skill5Req = 0;
		Skill6Req = 0;
		if (SkillInfos != null)
		{
			GuildSkillInfo[] skillInfos = SkillInfos;
			for (int i = 0; i < skillInfos.Length; i++)
			{
				skillInfos[i].ReqCount = 0;
			}
		}
	}

	public void SetSkillLevel(int skillIndex, byte value)
	{
		if (GameRoot.Instance.GuildSkillDataContainer.GetData(skillIndex) == null || SkillInfos == null)
		{
			return;
		}
		GuildSkillInfo[] skillInfos = SkillInfos;
		foreach (GuildSkillInfo guildSkillInfo in skillInfos)
		{
			if (guildSkillInfo.SkillIndex == skillIndex)
			{
				guildSkillInfo.SkillLevel = value;
			}
		}
	}
}
