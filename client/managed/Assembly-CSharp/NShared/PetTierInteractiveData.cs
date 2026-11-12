using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PetTierInteractiveData : IDataKeySelector<int>
{
	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public int SleepCoolTimeSec { get; set; }

	[Key(2)]
	public int RewardIndex { get; set; }

	[Key(3)]
	public int PetFeedCoolTimeSec { get; set; }

	[Key(4)]
	public int PetPatCoolTimeSec { get; set; }

	[Key(5)]
	public int PetPlayCoolTimeSec { get; set; }

	[Key(6)]
	public int PetFeedAddHappinessPoint { get; set; }

	[Key(7)]
	public int PetPatAddHappinessPoint { get; set; }

	[Key(8)]
	public int PetPlayAddHappinesPoint { get; set; }

	[Key(9)]
	public int PetMaxHappinessPoint { get; set; }

	int IDataKeySelector<int>.Key => Grade;
}
