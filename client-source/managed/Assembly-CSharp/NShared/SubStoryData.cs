using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SubStoryData : IDataKeySelector<ChapterDungeonKey2>
{
	private List<ForcedHeroInfo> _cachedHeroInfoList;

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public bool NoShowNodeInfo { get; set; }

	[Key(3)]
	public int CostumeIndex { get; set; }

	[Key(4)]
	public int CreatureIndex { get; set; }

	[Key(5)]
	public string[] SubStoryTactic { get; set; }

	[Key(6)]
	public int ReqCreatureIndex { get; set; }

	[Key(7)]
	public int ReqStar { get; set; }

	[Key(8)]
	public int ReqTranscended { get; set; }

	[Key(9)]
	public int ReqLevel { get; set; }

	[Key(10)]
	public int ReqChapterIndex { get; set; }

	[Key(11)]
	public int ReqDungeonIndex { get; set; }

	[Key(12)]
	public int FirstRewardIndex { get; set; }

	[Key(13)]
	public int UnlockCostumeIndex { get; set; }

	[Key(14)]
	public string IllustPath { get; set; }

	[Key(15)]
	public bool New { get; set; }

	[Key(16)]
	public int SubStoryEventIndex { get; set; }

	[Key(17)]
	public int[] ForcedHeroIndex { get; set; }

	[Key(18)]
	public int HeroStar1 { get; set; }

	[Key(19)]
	public int HeroLevel1 { get; set; }

	[Key(20)]
	public int HeroTier1 { get; set; }

	[Key(21)]
	public int HeroScaleR1 { get; set; }

	[Key(22)]
	public int HeroCostume1 { get; set; }

	[Key(23)]
	public int HeroWeaponIndex1 { get; set; }

	[Key(24)]
	public int[] HeroAddExtraSkill1 { get; set; }

	[Key(25)]
	public int HeroStar2 { get; set; }

	[Key(26)]
	public int HeroLevel2 { get; set; }

	[Key(27)]
	public int HeroTier2 { get; set; }

	[Key(28)]
	public int HeroScaleR2 { get; set; }

	[Key(29)]
	public int HeroCostume2 { get; set; }

	[Key(30)]
	public int HeroWeaponIndex2 { get; set; }

	[Key(31)]
	public int[] HeroAddExtraSkill2 { get; set; }

	[Key(32)]
	public int HeroStar3 { get; set; }

	[Key(33)]
	public int HeroLevel3 { get; set; }

	[Key(34)]
	public int HeroTier3 { get; set; }

	[Key(35)]
	public int HeroScaleR3 { get; set; }

	[Key(36)]
	public int HeroCostume3 { get; set; }

	[Key(37)]
	public int HeroWeaponIndex3 { get; set; }

	[Key(38)]
	public int[] HeroAddExtraSkill3 { get; set; }

	[Key(39)]
	public int HeroStar4 { get; set; }

	[Key(40)]
	public int HeroLevel4 { get; set; }

	[Key(41)]
	public int HeroTier4 { get; set; }

	[Key(42)]
	public int HeroScaleR4 { get; set; }

	[Key(43)]
	public int HeroCostume4 { get; set; }

	[Key(44)]
	public int HeroWeaponIndex4 { get; set; }

	[Key(45)]
	public int[] HeroAddExtraSkill4 { get; set; }

	public List<ForcedHeroInfo> GetForcedHeroInfoList()
	{
		if (_cachedHeroInfoList != null)
		{
			return _cachedHeroInfoList;
		}
		_cachedHeroInfoList = new List<ForcedHeroInfo>();
		ForcedHeroInfo forceHero = GetForceHero1();
		if (forceHero != null)
		{
			_cachedHeroInfoList.Add(forceHero);
		}
		ForcedHeroInfo forceHero2 = GetForceHero2();
		if (forceHero2 != null)
		{
			_cachedHeroInfoList.Add(forceHero2);
		}
		ForcedHeroInfo forceHero3 = GetForceHero3();
		if (forceHero3 != null)
		{
			_cachedHeroInfoList.Add(forceHero3);
		}
		ForcedHeroInfo forceHero4 = GetForceHero4();
		if (forceHero4 != null)
		{
			_cachedHeroInfoList.Add(forceHero4);
		}
		return _cachedHeroInfoList;
	}

	private CachedHeroInfo CreateCachedHeroInfo(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return null;
		}
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
		cachedHeroInfo.HeroIndex = heroInfo.HeroIndex;
		cachedHeroInfo.Level = heroInfo.Level;
		cachedHeroInfo.CostumeIndex = heroInfo.CostumeIndex;
		cachedHeroInfo.Star = heroInfo.Star;
		cachedHeroInfo.SetSkillLevel(1, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(2, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(3, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(4, heroInfo.Level);
		cachedHeroInfo.SetSkillLevel(5, heroInfo.Level);
		cachedHeroInfo.Validate();
		return cachedHeroInfo;
	}

	private ForcedHeroInfo GetForceHero1()
	{
		if (ForcedHeroIndex == null || ForcedHeroIndex.Length < 1)
		{
			return null;
		}
		HeroInfo heroInfo = new HeroInfo();
		heroInfo.HeroIndex = ForcedHeroIndex[0];
		heroInfo.Star = (sbyte)HeroStar1;
		heroInfo.Level = (sbyte)HeroLevel1;
		heroInfo.CostumeIndex = HeroCostume1;
		return new ForcedHeroInfo
		{
			CachedHeroInfo = CreateCachedHeroInfo(heroInfo),
			HeroScaleR = HeroScaleR1,
			HeroTier = HeroTier1,
			WeaponIndex = HeroWeaponIndex1,
			AddExtraSkills = HeroAddExtraSkill1
		};
	}

	private ForcedHeroInfo GetForceHero2()
	{
		if (ForcedHeroIndex == null || ForcedHeroIndex.Length < 2)
		{
			return null;
		}
		HeroInfo heroInfo = new HeroInfo();
		heroInfo.HeroIndex = ForcedHeroIndex[1];
		heroInfo.Star = (sbyte)HeroStar2;
		heroInfo.Level = (sbyte)HeroLevel2;
		heroInfo.CostumeIndex = HeroCostume2;
		return new ForcedHeroInfo
		{
			CachedHeroInfo = CreateCachedHeroInfo(heroInfo),
			HeroScaleR = HeroScaleR2,
			HeroTier = HeroTier2,
			WeaponIndex = HeroWeaponIndex2,
			AddExtraSkills = HeroAddExtraSkill2
		};
	}

	private ForcedHeroInfo GetForceHero3()
	{
		if (ForcedHeroIndex == null || ForcedHeroIndex.Length < 3)
		{
			return null;
		}
		HeroInfo heroInfo = new HeroInfo();
		heroInfo.HeroIndex = ForcedHeroIndex[2];
		heroInfo.Star = (sbyte)HeroStar3;
		heroInfo.Level = (sbyte)HeroLevel3;
		heroInfo.CostumeIndex = HeroCostume3;
		return new ForcedHeroInfo
		{
			CachedHeroInfo = CreateCachedHeroInfo(heroInfo),
			HeroScaleR = HeroScaleR3,
			HeroTier = HeroTier3,
			WeaponIndex = HeroWeaponIndex3,
			AddExtraSkills = HeroAddExtraSkill3
		};
	}

	private ForcedHeroInfo GetForceHero4()
	{
		if (ForcedHeroIndex == null || ForcedHeroIndex.Length < 4)
		{
			return null;
		}
		HeroInfo heroInfo = new HeroInfo();
		heroInfo.HeroIndex = ForcedHeroIndex[3];
		heroInfo.Star = (sbyte)HeroStar4;
		heroInfo.Level = (sbyte)HeroLevel4;
		heroInfo.CostumeIndex = HeroCostume4;
		return new ForcedHeroInfo
		{
			CachedHeroInfo = CreateCachedHeroInfo(heroInfo),
			HeroScaleR = HeroScaleR4,
			HeroTier = HeroTier4,
			WeaponIndex = HeroWeaponIndex4,
			AddExtraSkills = HeroAddExtraSkill4
		};
	}

	public override string ToString()
	{
		return "ChapterIndex : " + ChapterIndex + "\nDungeonIndex : " + DungeonIndex + "\nCreatureIndex : " + CreatureIndex + "\nReqCreatureIndex : " + ReqCreatureIndex + "\nCostumeIndex : " + CostumeIndex + "\nReqStar : " + ReqStar + "\nReqTrancendance : " + ReqTranscended + "\nReqChapter : " + ReqChapterIndex + "\nReqDungeon : " + ReqDungeonIndex + "\n";
	}
}
