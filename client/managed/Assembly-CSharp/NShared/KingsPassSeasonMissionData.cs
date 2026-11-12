using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class KingsPassSeasonMissionData : IDataKeySelector<KingsPassMissionKey2>
{
	KingsPassMissionKey2 IDataKeySelector<KingsPassMissionKey2>.Key => new KingsPassMissionKey2(SeasonIndex, MissionIndex);

	[Key(0)]
	public int SeasonIndex { get; set; }

	[Key(1)]
	public int MissionIndex { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public KingsPassType Type { get; set; }

	[Key(4)]
	public int Level { get; set; }

	[Key(5)]
	public string AdditionalRewardPriceType { get; set; }

	[Key(6)]
	public int AdditionalRewardPrice { get; set; }

	[Key(7)]
	public int RewardIndex { get; set; }

	[Key(8)]
	public string StateType { get; set; }

	[Key(9)]
	public int[] StateValue { get; set; }

	[Key(10)]
	public int ReqProgress { get; set; }

	[Key(11)]
	public bool AutoComplete { get; set; }

	public override string ToString()
	{
		return "SeasonIndex=" + SeasonIndex + "MissionIndex =" + MissionIndex;
	}
}
