namespace NShared;

[JsonSerializable(Javascript = true)]
public class RouletteRewardInfo : IAccountAttrKey<int>
{
	public const int MaxReward = 3;

	private DbRewardData[] rewardList;

	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public int Section { get; set; }

	public RewardType Type1 { get; set; }

	public int Value1 { get; set; }

	public int Count1 { get; set; }

	public RewardType Type2 { get; set; }

	public int Value2 { get; set; }

	public int Count2 { get; set; }

	public RewardType Type3 { get; set; }

	public int Value3 { get; set; }

	public int Count3 { get; set; }

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
