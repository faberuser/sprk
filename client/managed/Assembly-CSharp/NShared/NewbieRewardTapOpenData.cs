using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewbieRewardTapOpenData : IDataKeySelector<int>
{
	[Key(0)]
	public int TabIndex { get; set; }

	[Key(1)]
	public string TabName { get; set; }

	[Key(2)]
	public NewbieRewardType TabType { get; set; }

	[Key(3)]
	public string[] Condition { get; set; }

	int IDataKeySelector<int>.Key => TabIndex;

	public override string ToString()
	{
		return "Index=" + TabIndex;
	}
}
