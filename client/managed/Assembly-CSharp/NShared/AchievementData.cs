using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class AchievementData : IDataKeySelector<AchievementKey2>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Step { get; set; }

	[Key(2)]
	public int ShowIndex { get; set; }

	[Key(3)]
	public AchievementType Type { get; set; }

	[Key(4)]
	public AchievementSubType SubType { get; set; }

	[Key(5)]
	public string EventName { get; set; }

	[Key(6)]
	public AchievementCategoryType Category { get; set; }

	[Key(7)]
	public int SubGroup { get; set; }

	[Key(8)]
	public DisplayProgressType DisplayProgress { get; set; }

	[Key(9)]
	public string CheckType { get; set; }

	[Key(10)]
	public AchievementReqType ReqType { get; set; }

	[Key(11)]
	public string ReqValue { get; set; }

	[Key(12)]
	public bool ReqValueDecrease { get; set; }

	[Key(13)]
	public AchievementReqSubType ReqSub1Type { get; set; }

	[Key(14)]
	public long ReqSub1Value { get; set; }

	[Key(15)]
	public AchievementReqSubType ReqSub2Type { get; set; }

	[Key(16)]
	public long ReqSub2Value { get; set; }

	[Key(17)]
	public AchievementRewardType RewardType { get; set; }

	[Key(18)]
	public string RewardCode { get; set; }

	[Key(19)]
	public int RewardValue { get; set; }

	[Key(20)]
	public AchievementRewardType Reward2Type { get; set; }

	[Key(21)]
	public string Reward2Code { get; set; }

	[Key(22)]
	public int Reward2Value { get; set; }

	[Key(23)]
	public int ResetAchievement { get; set; }

	[Key(24)]
	public string MoveTaskType { get; set; }

	[Key(25)]
	public string[] MoveTaskValue { get; set; }

	[Key(26)]
	public string GPGSAchievementId { get; set; }

	[Key(27)]
	public bool OnlyDisplayClearFlag { get; set; }

	[Key(28)]
	public int RepeatValue { get; set; }

	[Key(29)]
	public int MenuIndex { get; set; }

	[Key(30)]
	public string[] OpenCondition { get; set; }

	[Key(31)]
	public int TutorialIndex { get; set; }

	[Key(32)]
	public bool DailyReset { get; set; }

	[Key(33)]
	public bool WeeklyReset { get; set; }

	[Key(34)]
	public int[] MonthlyResetDate { get; set; }

	[Key(35)]
	public EventBonusType LimitedEventMissionType { get; set; }

	[Key(36)]
	public string HandlerName { get; set; }

	[Key(37)]
	public string[] HandlerCondition { get; set; }

	[Key(38)]
	public string Title { get; set; }

	[Key(39)]
	public string[] Variable1 { get; set; }

	[Key(40)]
	public string Desc { get; set; }

	[Key(41)]
	public string[] Variable2 { get; set; }

	[Key(42)]
	public string Goal { get; set; }

	[Key(43)]
	public string[] Variable3 { get; set; }

	AchievementKey2 IDataKeySelector<AchievementKey2>.Key => new AchievementKey2(Index, Step);

	public override string ToString()
	{
		return this.GETKEY().ToString();
	}

	public static bool ValidReward(AchievementReward reward)
	{
		if (reward == null)
		{
			return false;
		}
		return ValidReward(reward.RewardType, reward.RewardCode, reward.RewardValue);
	}

	public static bool ValidReward(AchievementRewardType rewardType, string RewardCode, int RewardValue)
	{
		switch (rewardType)
		{
		case AchievementRewardType.None:
			return false;
		case AchievementRewardType.Item:
			if (GameRoot.Instance.ItemDataContainer.GetDataByCode(RewardCode) == null)
			{
				return false;
			}
			break;
		}
		if (rewardType != AchievementRewardType.GoldTeamLevel && rewardType != AchievementRewardType.StaminaTeamLevel && RewardValue <= 0)
		{
			return false;
		}
		return true;
	}

	public List<AchievementReward> GetAchievementRewardList(bool isSorted = true)
	{
		List<AchievementReward> list = new List<AchievementReward>();
		if (ValidReward(RewardType, RewardCode, RewardValue))
		{
			AchievementReward item = new AchievementReward
			{
				RewardType = RewardType,
				RewardCode = RewardCode,
				RewardValue = RewardValue
			};
			list.Add(item);
		}
		if (ValidReward(Reward2Type, Reward2Code, Reward2Value))
		{
			AchievementReward item2 = new AchievementReward
			{
				RewardType = Reward2Type,
				RewardCode = Reward2Code,
				RewardValue = Reward2Value
			};
			list.Add(item2);
		}
		if (!isSorted)
		{
			return list;
		}
		list.Sort(delegate(AchievementReward l, AchievementReward r)
		{
			int value = (int)((l.RewardType != AchievementRewardType.TeamExp) ? l.RewardType : AchievementRewardType.None);
			int num = ((int)((r.RewardType != AchievementRewardType.TeamExp) ? r.RewardType : AchievementRewardType.None)).CompareTo(value);
			return (num != 0) ? num : l.RewardType.CompareTo(r.RewardType);
		});
		return list;
	}
}
