using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class TowerFloorData : IDataKeySelector<TowerFloorKey>
{
	TowerFloorKey IDataKeySelector<TowerFloorKey>.Key => new TowerFloorKey(TowerIndex, Floor);

	[Key(0)]
	public int TowerIndex { get; set; }

	[Key(1)]
	public int Floor { get; set; }

	[Key(2)]
	public string OpenTime { get; set; }

	[Key(3)]
	public int ChapterIndex { get; set; }

	[Key(4)]
	public int DungeonIndex { get; set; }

	[Key(5)]
	public FloorDungeonSelectType DungeonSelectType { get; set; }

	[Key(6)]
	public int[] DungeonIndices { get; set; }

	[Key(7)]
	public int ReqTeamLevel { get; set; }

	[Key(8)]
	public int ReqChapterIndex { get; set; }

	[Key(9)]
	public int ReqDungeonIndex { get; set; }

	[Key(10)]
	public int[] ReqClearedChapterIndex { get; set; }

	[Key(11)]
	public int ReqClearedChapterDifficulty { get; set; }

	[Key(12)]
	public int ReqClass { get; set; }

	[Key(13)]
	public RewardType BattleCostType { get; set; }

	[Key(14)]
	public int BattleStartCost { get; set; }

	[Key(15)]
	public int BattleEndCost { get; set; }

	[Key(16)]
	public int[] RewardIndex { get; set; }

	[Key(17)]
	public int[] RepeatRewardIndex { get; set; }

	public override string ToString()
	{
		return "TowerFloorData Index=" + TowerIndex + " Floor=" + Floor;
	}

	public StaminaType GetReqStaminaType(SaveStaminaBoosterInfo saveStaminaInfo = null, StaminaType defaultType = StaminaType.Chicken)
	{
		if (GetReqStamina(saveStaminaInfo, out var staminaType, out var _))
		{
			return staminaType;
		}
		return defaultType;
	}

	public bool GetReqStamina(SaveStaminaBoosterInfo saveStaminaInfo, out StaminaType staminaType, out long staminaValue)
	{
		if (BattleCostType == RewardType.Gold)
		{
			staminaType = StaminaType.Gold;
			int num = BattleStartCost + BattleEndCost;
			staminaValue = num;
			return true;
		}
		if (BattleCostType == RewardType.Gem)
		{
			staminaType = StaminaType.Gem;
			int num2 = BattleStartCost + BattleEndCost;
			staminaValue = num2;
			return true;
		}
		staminaType = StaminaType.Chicken;
		int num3 = 0;
		staminaValue = num3;
		int chapterIndex = ChapterIndex;
		int dungeonIndex = DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			staminaType = data.ReqStaminaType;
			int staminaOfDifficulty = data.GetStaminaOfDifficulty(saveStaminaInfo);
			staminaValue = staminaOfDifficulty;
			return true;
		}
		return false;
	}
}
