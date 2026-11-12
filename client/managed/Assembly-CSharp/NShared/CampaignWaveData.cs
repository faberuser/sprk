using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignWaveData : IDataKeySelector<CampaignWaveKey2>
{
	[Flags]
	public enum EWaveFlag
	{
		Ally = 1,
		Hide = 2,
		WaveMember = 4
	}

	public class EnvironmentProjectileInfo
	{
		public int OffsetMx { get; set; }

		public int OffsetMz { get; set; }

		public TeamType TeamType { get; set; }

		public int ProjectileIndex { get; set; }
	}

	public class WaveAreaInfo
	{
		public int AreaGroupIndex { get; set; }

		public bool AreaPositionIsRelative { get; set; }

		public int AreaPositionMx { get; set; }

		public int AreaSizeMx { get; set; }

		public int RightTeamOffsetMx { get; set; }
	}

	public class CreatureReadData
	{
		public sbyte Star { get; set; }

		public short Level { get; set; }

		public int Tier { get; set; }

		public MonsterGradeType Grade { get; set; }

		public EWaveFlag Flag { get; set; }

		public string VisualInfo { get; set; }

		public int OffsetMx { get; set; }

		public int OffsetMz { get; set; }

		public short DelayMs { get; set; }

		public int ScaleR { get; set; }

		public int HeightMm { get; set; }

		public sbyte SkillOpen { get; set; }

		public int[] AddExtraSkill { get; set; }

		public int GroupIndex { get; set; }

		public long MinHp { get; set; }
	}

	public abstract class CreatureDetailInfo
	{
		[IgnoreMember]
		public abstract int Index { get; }

		[IgnoreMember]
		public abstract int Star { get; }

		[IgnoreMember]
		public abstract int Level { get; }

		[IgnoreMember]
		public abstract int Tier { get; }

		[IgnoreMember]
		public abstract MonsterGradeType Grade { get; }

		[IgnoreMember]
		public abstract EWaveFlag WaveFlag { get; }

		[IgnoreMember]
		public abstract string VisualInfo { get; }

		[IgnoreMember]
		public abstract int OffsetMx { get; }

		[IgnoreMember]
		public abstract int OffsetMz { get; }

		[IgnoreMember]
		public abstract int DelayMs { get; }

		[IgnoreMember]
		public abstract int ScaleR { get; }

		[IgnoreMember]
		public abstract int HeightMm { get; }

		[IgnoreMember]
		public abstract int SkillOpen { get; }

		[IgnoreMember]
		public abstract int[] AddExtraSkill { get; }

		[IgnoreMember]
		public abstract int WeaponIndex { get; }

		[IgnoreMember]
		public abstract int GroupIndex { get; }

		[IgnoreMember]
		public abstract long ForceMinHp { get; }

		[IgnoreMember]
		public abstract bool IsBoss { get; }

		[IgnoreMember]
		public abstract int WaveSlotIndex { get; }

		[IgnoreMember]
		public bool IsAlly => (WaveFlag & EWaveFlag.Ally) == EWaveFlag.Ally;

		[IgnoreMember]
		public bool HidePortrait => (WaveFlag & EWaveFlag.Hide) == EWaveFlag.Hide;

		[IgnoreMember]
		public bool IsWaveMember => (WaveFlag & EWaveFlag.WaveMember) == EWaveFlag.WaveMember;

		public bool IsAvailable(bool useGroupFilter, int targetGroupIndex)
		{
			if (useGroupFilter)
			{
				return GroupIndex == targetGroupIndex;
			}
			return true;
		}

		public CreatureDetailWritable CloneWritable()
		{
			return new CreatureDetailWritable(Index, Star, Level, Tier, Grade, IsBoss, WaveFlag, WaveSlotIndex, VisualInfo, OffsetMx, OffsetMz, DelayMs, ScaleR, HeightMm, SkillOpen, AddExtraSkill, WeaponIndex, GroupIndex, ForceMinHp);
		}
	}

	[MessagePackObject(false)]
	public class CreatureDetailWritable : CreatureDetailInfo
	{
		[IgnoreMember]
		public override int Index => _Index;

		[IgnoreMember]
		public override int Star => _Star;

		[IgnoreMember]
		public override int Level => _Level;

		[IgnoreMember]
		public override int Tier => _Tier;

		[IgnoreMember]
		public override MonsterGradeType Grade => _Grade;

		[IgnoreMember]
		public override EWaveFlag WaveFlag => _WaveFlag;

		[IgnoreMember]
		public override string VisualInfo => _VisualInfo;

		[IgnoreMember]
		public override int OffsetMx => _OffsetMx;

		[IgnoreMember]
		public override int OffsetMz => _OffsetMz;

		[IgnoreMember]
		public override int DelayMs => _DelayMs;

		[IgnoreMember]
		public override int ScaleR => _ScaleR;

		[IgnoreMember]
		public override int HeightMm => _HeightMm;

		[IgnoreMember]
		public override int SkillOpen => _SkillOpen;

		[IgnoreMember]
		public override int[] AddExtraSkill => _AddExtraSkill;

		[IgnoreMember]
		public override int WeaponIndex => _WeaponIndex;

		[IgnoreMember]
		public override int GroupIndex => _GroupIndex;

		[IgnoreMember]
		public override long ForceMinHp => _ForceMinHp;

		[IgnoreMember]
		public override bool IsBoss => _IsBoss;

		[IgnoreMember]
		public override int WaveSlotIndex => _WaveSlotIndex;

		[Key(0)]
		public int _Index { get; set; }

		[Key(1)]
		public int _Star { get; set; }

		[Key(2)]
		public int _Level { get; set; }

		[Key(3)]
		public int _Tier { get; set; }

		[Key(4)]
		public MonsterGradeType _Grade { get; set; }

		[Key(5)]
		public EWaveFlag _WaveFlag { get; set; }

		[Key(6)]
		public string _VisualInfo { get; set; }

		[Key(7)]
		public int _OffsetMx { get; set; }

		[Key(8)]
		public int _OffsetMz { get; set; }

		[Key(9)]
		public int _DelayMs { get; set; }

		[Key(10)]
		public int _ScaleR { get; set; }

		[Key(11)]
		public int _HeightMm { get; set; }

		[Key(12)]
		public int _SkillOpen { get; set; }

		[Key(13)]
		public int[] _AddExtraSkill { get; set; }

		[Key(14)]
		public int _WeaponIndex { get; set; }

		[Key(15)]
		public int _GroupIndex { get; set; }

		[Key(16)]
		public long _ForceMinHp { get; set; }

		[Key(17)]
		public bool _IsBoss { get; set; }

		[Key(18)]
		public int _WaveSlotIndex { get; set; }

		public CreatureDetailWritable()
		{
		}

		public CreatureDetailWritable(int index, int star, int level, int tier, MonsterGradeType grade, bool isBoss, EWaveFlag waveFlag, int waveSlotIndex, string visualInfo, int offsetMx, int offsetMz, int delayMs, int scaleR, int heightMm, int skillOpen, int[] addExtraSkill, int weaponIndex, int groupIndex, long forceMinHp)
		{
			_Index = index;
			_Star = star;
			_Level = level;
			_Tier = tier;
			_Grade = grade;
			_IsBoss = isBoss;
			_WaveFlag = waveFlag;
			_WaveSlotIndex = waveSlotIndex;
			_VisualInfo = visualInfo;
			_OffsetMx = offsetMx;
			_OffsetMz = offsetMz;
			_DelayMs = delayMs;
			_ScaleR = scaleR;
			_HeightMm = heightMm;
			_SkillOpen = skillOpen;
			_AddExtraSkill = addExtraSkill;
			_WeaponIndex = weaponIndex;
			_GroupIndex = groupIndex;
			_ForceMinHp = forceMinHp;
		}
	}

	CampaignWaveKey2 IDataKeySelector<CampaignWaveKey2>.Key => new CampaignWaveKey2(ChapterIndex, DungeonIndex, Difficulty, Scenario, WaveIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public DungeonDifficultyType Difficulty { get; set; }

	[Key(3)]
	public bool Scenario { get; set; }

	[Key(4)]
	public int WaveIndex { get; set; }

	[Key(5)]
	public sbyte Star { get; set; }

	[Key(6)]
	public short Level { get; set; }

	[Key(7)]
	public int Tier { get; set; }

	[Key(8)]
	public sbyte SkillOpen { get; set; }

	[Key(9)]
	public string BgmCode { get; set; }

	[Key(10)]
	public WinDeciderType WinDeciderType { get; set; }

	[Key(11)]
	public string[] WinDeciderValue { get; set; }

	[Key(12)]
	public int[] AddHeroExtraSkill { get; set; }

	[Key(13)]
	public int AreaGroupIndex { get; set; }

	[Key(14)]
	public int AreaPositionMx { get; set; }

	[Key(15)]
	public bool AreaPositionIsRelative { get; set; }

	[Key(16)]
	public int AreaSizeMx { get; set; }

	[Key(17)]
	public int RightTeamOffsetMx { get; set; }

	[Key(18)]
	public int AreaGroupIndex2 { get; set; }

	[Key(19)]
	public int AreaPositionMx2 { get; set; }

	[Key(20)]
	public bool AreaPositionIsRelative2 { get; set; }

	[Key(21)]
	public int AreaSizeMx2 { get; set; }

	[Key(22)]
	public int RightTeamOffsetMx2 { get; set; }

	[Key(23)]
	public DelayedSpawnType DelayedSpawnType { get; set; }

	[Key(24)]
	public string[] EnvironmentProjectile1 { get; set; }

	[Key(25)]
	public string[] EnvironmentProjectile2 { get; set; }

	[Key(26)]
	public string[] EnvironmentProjectile3 { get; set; }

	[Key(27)]
	public string[] EnvironmentProjectile4 { get; set; }

	[Key(28)]
	public string[] EnvironmentProjectile5 { get; set; }

	[Key(29)]
	public string SoundName { get; set; }

	[Key(30)]
	public List<CreatureDetailWritable> BossList { get; set; }

	[Key(31)]
	public List<CreatureDetailWritable> CreatureList { get; set; }

	[IgnoreMember]
	public int BossCreatureIndex
	{
		get
		{
			if (BossList == null)
			{
				return 0;
			}
			if (BossList.Count == 0)
			{
				return 0;
			}
			return BossList[0].Index;
		}
	}

	[IgnoreMember]
	public string BossVisualInfo
	{
		get
		{
			if (BossList == null)
			{
				return string.Empty;
			}
			if (BossList.Count == 0)
			{
				return string.Empty;
			}
			return BossList[0].VisualInfo;
		}
	}

	public override string ToString()
	{
		return "[Chapter=" + ChapterIndex + " Dungeon" + DungeonIndex + " Difficulty=" + Difficulty.ToString() + " Wave=" + WaveIndex + "]";
	}

	public List<int> GetCreatureIndexList(bool includeBoss = true, bool includeAlly = true)
	{
		List<int> list = new List<int>();
		if (includeBoss && BossList != null)
		{
			list.AddRange(BossList.Select((CreatureDetailWritable v) => v.Index));
		}
		if (CreatureList != null)
		{
			list.AddRange(from v in CreatureList
				where includeAlly || !v.IsAlly
				select v.Index);
		}
		return list;
	}

	public List<CreatureDetailInfo> GetCreatureList(bool includeBoss = true, bool useGroupFilter = false, int targetGroupIndex = 0)
	{
		List<CreatureDetailInfo> list = new List<CreatureDetailInfo>();
		if (includeBoss && BossList != null)
		{
			list.AddRange(from v in BossList
				where v.IsAvailable(useGroupFilter, targetGroupIndex)
				select __Extension_Method_CSharp_Class_Common__.Upcasting((CreatureDetailInfo)v));
		}
		if (CreatureList != null)
		{
			list.AddRange(from v in CreatureList
				where v.IsAvailable(useGroupFilter, targetGroupIndex)
				select __Extension_Method_CSharp_Class_Common__.Upcasting((CreatureDetailInfo)v));
		}
		return list;
	}

	public bool GetCreatureHeroInfo(Func<CreatureDetailInfo, bool> selector, out CreatureDetailInfo foundDetailInfo, out HeroInfo monsterInfo)
	{
		foundDetailInfo = null;
		monsterInfo = null;
		List<CreatureDetailInfo> creatureList = GetCreatureList();
		for (int i = 0; i < creatureList.Count; i++)
		{
			CreatureDetailInfo creatureDetailInfo = creatureList[i];
			if (selector(creatureDetailInfo))
			{
				foundDetailInfo = creatureDetailInfo;
				break;
			}
		}
		if (foundDetailInfo == null)
		{
			return false;
		}
		monsterInfo = CreateHeroInfoFromDetailInfo(foundDetailInfo);
		return true;
	}

	public static HeroInfo CreateHeroInfoFromDetailInfo(CreatureDetailInfo detailInfo)
	{
		short num = (short)detailInfo.Level;
		int skillOpen = detailInfo.SkillOpen;
		return new HeroInfo
		{
			HeroIndex = detailInfo.Index,
			Level = num,
			Star = (sbyte)detailInfo.Star,
			SkillLevel1 = (short)((skillOpen == 0 || skillOpen >= 1) ? num : 0),
			SkillLevel2 = (short)((skillOpen == 0 || skillOpen >= 2) ? num : 0),
			SkillLevel3 = (short)((skillOpen == 0 || skillOpen >= 3) ? num : 0),
			SkillLevel4 = (short)((skillOpen == 0 || skillOpen >= 4) ? num : 0),
			SkillLevel5 = (short)((skillOpen == 0 || skillOpen >= 5) ? num : 0)
		};
	}

	public EnvironmentProjectileInfo GetEnvironmentProjectileInfo(int no)
	{
		return no switch
		{
			1 => CreateEnvironmentProjectileInfo(EnvironmentProjectile1), 
			2 => CreateEnvironmentProjectileInfo(EnvironmentProjectile2), 
			3 => CreateEnvironmentProjectileInfo(EnvironmentProjectile3), 
			4 => CreateEnvironmentProjectileInfo(EnvironmentProjectile4), 
			5 => CreateEnvironmentProjectileInfo(EnvironmentProjectile5), 
			_ => null, 
		};
	}

	private EnvironmentProjectileInfo CreateEnvironmentProjectileInfo(string[] values)
	{
		if (values == null || values.Length == 0)
		{
			return null;
		}
		return new EnvironmentProjectileInfo
		{
			ProjectileIndex = ArgParser.GetValueInt(values, 0),
			TeamType = ArgParser.GetValueEnum(values, 1, TeamType.Left),
			OffsetMx = ArgParser.GetValueInt(values, 2),
			OffsetMz = ArgParser.GetValueInt(values, 3)
		};
	}

	public WaveAreaInfo GetWaveAreaInfoInfo(int groupIndex)
	{
		if (groupIndex == AreaGroupIndex)
		{
			return CreateWaveAreaInfo(AreaGroupIndex, AreaPositionIsRelative, AreaPositionMx, AreaSizeMx, RightTeamOffsetMx);
		}
		if (groupIndex == AreaGroupIndex2)
		{
			return CreateWaveAreaInfo(AreaGroupIndex2, AreaPositionIsRelative2, AreaPositionMx2, AreaSizeMx2, RightTeamOffsetMx2);
		}
		return null;
	}

	private WaveAreaInfo CreateWaveAreaInfo(int groupIndex, bool isRelative, int positionMx, int sizeMx, int offset)
	{
		return new WaveAreaInfo
		{
			AreaGroupIndex = groupIndex,
			AreaPositionIsRelative = isRelative,
			AreaPositionMx = positionMx,
			AreaSizeMx = sizeMx,
			RightTeamOffsetMx = offset
		};
	}
}
