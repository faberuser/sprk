using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class AccumulateLoginRewardData : IDataKeySelector<int>
{
	public const int MaxReward = 3;

	private DbRewardData[] rewardList;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int Day { get; set; }

	[Key(2)]
	public RewardType Type1 { get; set; }

	[Key(3)]
	public int Value1 { get; set; }

	[Key(4)]
	public int Count1 { get; set; }

	[Key(5)]
	public RewardType Type2 { get; set; }

	[Key(6)]
	public int Value2 { get; set; }

	[Key(7)]
	public int Count2 { get; set; }

	[Key(8)]
	public RewardType Type3 { get; set; }

	[Key(9)]
	public int Value3 { get; set; }

	[Key(10)]
	public int Count3 { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public DbRewardData[] GerRewardDataAry()
	{
		if (rewardList == null)
		{
			rewardList = new DbRewardData[3];
			for (int i = 0; i < rewardList.Length; i++)
			{
				rewardList[i] = new DbRewardData();
			}
			if (Type1 != RewardType.None)
			{
				rewardList[0].Type = Type1;
				rewardList[0].Value = Value1;
				rewardList[0].Count = Count1;
			}
			if (Type2 != RewardType.None)
			{
				rewardList[1].Type = Type2;
				rewardList[1].Value = Value2;
				rewardList[1].Count = Count2;
			}
			if (Type3 != RewardType.None)
			{
				rewardList[2].Type = Type3;
				rewardList[2].Value = Value3;
				rewardList[2].Count = Count3;
			}
		}
		return rewardList;
	}
}
