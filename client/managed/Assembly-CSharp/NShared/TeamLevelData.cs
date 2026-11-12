using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable]
public class TeamLevelData : IDataKeySelector<int>
{
	[Key(10)]
	public long TotalExp;

	int IDataKeySelector<int>.Key => Level;

	[Key(0)]
	public int Level { get; set; }

	[Key(1)]
	public long LocalExp { get; set; }

	[Key(2)]
	public int MaxStamina { get; set; }

	[Key(3)]
	public int RechargeStaminaSec { get; set; }

	[Key(4)]
	public int DailyBonusStamina { get; set; }

	[Key(5)]
	public int ExpBonusPercentage { get; set; }

	[Key(6)]
	public int GoldBonusPercentage { get; set; }

	[Key(7)]
	public int DailyGold { get; set; }

	[Key(8)]
	public int TeamLevelBuffOptionIndex { get; set; }

	[Key(9)]
	public int TeamLevelBuffBonusOptionIndex { get; set; }

	public override string ToString()
	{
		return "Level=" + Level;
	}
}
