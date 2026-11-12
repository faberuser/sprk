using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewbieRewardProductMissionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public NewbieRewardProvideType ProvideType { get; set; }

	[Key(3)]
	public string[] Condition { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public int ProductGroupIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
