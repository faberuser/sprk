using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ContentsOpenConditionData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public ContentsType ContentsType { get; set; }

	[Key(2)]
	public int[] ConditionValue { get; set; }

	[Key(3)]
	public string EntryMenuCondition { get; set; }

	[Key(4)]
	public int OpenConditionMainMission { get; set; }

	[Key(5)]
	public bool OpenText { get; set; }

	[Key(6)]
	public string OpenTextResorce { get; set; }

	[Key(7)]
	public string OpenLocalKey { get; set; }

	[Key(8)]
	public string[] OpenLocalKeyVariable { get; set; }

	[Key(9)]
	public int OpenConditionSubQuest { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
