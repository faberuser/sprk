using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ChallengeRaidClearRewardData : IDataKeySelector<ChallengeRaidClearRewardKey>
{
	ChallengeRaidClearRewardKey IDataKeySelector<ChallengeRaidClearRewardKey>.Key => new ChallengeRaidClearRewardKey(ChallengeRaidIndex, AchievementIndex);

	[Key(0)]
	public int ChallengeRaidIndex { get; set; }

	[Key(1)]
	public int AchievementIndex { get; set; }

	[Key(2)]
	public int RaidLevel { get; set; }

	[Key(3)]
	public int ClearCount { get; set; }

	[Key(4)]
	public AchievementRewardType RewardType { get; set; }

	[Key(5)]
	public string RewardCode { get; set; }

	[Key(6)]
	public int RewardValue { get; set; }

	[Key(7)]
	public AchievementRewardType Reward2Type { get; set; }

	[Key(8)]
	public string Reward2Code { get; set; }

	[Key(9)]
	public int Reward2Value { get; set; }

	[Key(10)]
	public AchievementRewardType Reward3Type { get; set; }

	[Key(11)]
	public string Reward3Code { get; set; }

	[Key(12)]
	public int Reward3Value { get; set; }

	public override string ToString()
	{
		return "ChallengeRaidClearRewardData Index=" + ChallengeRaidIndex;
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

	public List<AchievementReward> GetAchievementRewardList()
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
		list.Sort((AchievementReward l, AchievementReward r) => l.RewardType.CompareTo(r.RewardType));
		return list;
	}
}
