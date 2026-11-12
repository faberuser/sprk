using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignChapterData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public ChapterType Type { get; set; }

	[Key(2)]
	public int DailyEnterMaxCount { get; set; }

	[Key(3)]
	public bool MainField { get; set; }

	[Key(4)]
	public bool NodeOpenEffect { get; set; }

	[Key(5)]
	public bool IsOpen { get; set; }

	[Key(6)]
	public string Name { get; set; }

	[Key(7)]
	public string Map { get; set; }

	[Key(8)]
	public int MapSizeX { get; set; }

	[Key(9)]
	public int MapSizeY { get; set; }

	[Key(10)]
	public float[] ShadowDirection { get; set; }

	[Key(11)]
	public int StartNodeIndex { get; set; }

	[Key(12)]
	public int LabelScale { get; set; }

	[Key(13)]
	public string[] UserStatType { get; set; }

	[Key(14)]
	public string[] UserStatTypeIconSprite { get; set; }

	[Key(15)]
	public string[] ResultInfoIconSprite { get; set; }

	[Key(16)]
	public StaminaType StaminaType { get; set; }

	[Key(17)]
	public int ReqStamina { get; set; }

	[Key(18)]
	public StaminaType SubStaminaType { get; set; }

	[Key(19)]
	public bool OccurRaid { get; set; }

	[Key(20)]
	public int CreatureExp { get; set; }

	[Key(21)]
	public string BgmCode { get; set; }

	[Key(22)]
	public string Description { get; set; }

	[Key(23)]
	public string BasePortalImageName { get; set; }

	[Key(24)]
	public int ReqChapterIndex { get; set; }

	[Key(25)]
	public int ReqDungeonIndex { get; set; }

	[Key(26)]
	public DungeonDifficultyType ReqDungeonDifficulty { get; set; }

	[Key(27)]
	public int ForcedAvatarIndex { get; set; }

	[Key(28)]
	public string BeforeEnterMap { get; set; }

	[Key(29)]
	public string EnterCheckerName { get; set; }

	[Key(30)]
	public string[] Values { get; set; }

	[Key(31)]
	public RewardType EasyRewardType { get; set; }

	[Key(32)]
	public string EasyRewardCode { get; set; }

	[Key(33)]
	public int EasyRewardCount { get; set; }

	[Key(34)]
	public int[] EasyChapterHiddenReward { get; set; }

	[Key(35)]
	public RewardType NormalRewardType { get; set; }

	[Key(36)]
	public string NormalRewardCode { get; set; }

	[Key(37)]
	public int NormalRewardCount { get; set; }

	[Key(38)]
	public int[] NormalChapterHiddenReward { get; set; }

	[Key(39)]
	public RewardType HardRewardType { get; set; }

	[Key(40)]
	public string HardRewardCode { get; set; }

	[Key(41)]
	public int HardRewardCount { get; set; }

	[Key(42)]
	public int[] HardChapterHiddenReward { get; set; }

	[Key(43)]
	public RewardType HellRewardType { get; set; }

	[Key(44)]
	public string HellRewardCode { get; set; }

	[Key(45)]
	public int HellRewardCount { get; set; }

	[Key(46)]
	public int[] HellChapterHiddenReward { get; set; }

	[Key(47)]
	public int HellMode_ReqChapterIndex { get; set; }

	[Key(48)]
	public int HellMode_ReqDungeonIndex { get; set; }

	[Key(49)]
	public DungeonDifficultyType HellMode_ReqDungeonDifficulty { get; set; }

	[Key(50)]
	public ERightMenuButton[] ShowMenuButtons { get; set; }

	[Key(51)]
	public DungeonDifficultyType MaxDifficulty { get; set; }

	[Key(52)]
	public int AvatarScale { get; set; }

	[Key(53)]
	public GuildConstructionResourceTopmostMenuType GuildResourceMenuType { get; set; }

	[Key(54)]
	public bool MultiAvatarUse { get; set; }

	[Key(55)]
	public EMapArrangement MapArrangement { get; set; }

	public bool RewardExists()
	{
		return EasyRewardType != RewardType.None;
	}

	public string GetUserStatTypeIconSprite(string statType)
	{
		int num = 0;
		if (UserStatType != null && UserStatType.Length != 0)
		{
			for (int i = 0; i < UserStatType.Length; i++)
			{
				if (UserStatType[i].Equals(statType))
				{
					num = i;
				}
			}
		}
		if (UserStatTypeIconSprite != null && UserStatTypeIconSprite.Length >= num)
		{
			return UserStatTypeIconSprite[num];
		}
		return null;
	}

	public string GetResultInfoIconSprite(string statType)
	{
		int num = 0;
		if (UserStatType != null && UserStatType.Length != 0)
		{
			for (int i = 0; i < UserStatType.Length; i++)
			{
				if (UserStatType[i].Equals(statType))
				{
					num = i;
				}
			}
		}
		if (ResultInfoIconSprite != null && ResultInfoIconSprite.Length >= num)
		{
			return ResultInfoIconSprite[num];
		}
		return null;
	}

	public string GetChapterName()
	{
		return GameRoot.LocalizeProxy.GetString(Name);
	}
}
