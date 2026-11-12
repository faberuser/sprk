using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PlayerArchiveData : IDataKeySelector<PlayerArchiveDataKey>
{
	[Key(0)]
	public int MainIndex { get; set; }

	[Key(1)]
	public int SubIndex { get; set; }

	[Key(2)]
	public int ShowIndex { get; set; }

	[Key(3)]
	public ArchiveMainType ArchiveMainType { get; set; }

	[Key(4)]
	public ArchiveSubType ArchiveSubType { get; set; }

	[Key(5)]
	public ArchiveSubType2 ArchiveSubType2 { get; set; }

	[Key(6)]
	public string ConditionKey1 { get; set; }

	[Key(7)]
	public string ConditionCompare1 { get; set; }

	[Key(8)]
	public ItemType[] ConditionValue1 { get; set; }

	[Key(9)]
	public string ConditionKey2 { get; set; }

	[Key(10)]
	public string ConditionCompare2 { get; set; }

	[Key(11)]
	public int[] ConditionValue2 { get; set; }

	[Key(12)]
	public string ConditionKey3 { get; set; }

	[Key(13)]
	public string ConditionCompare3 { get; set; }

	[Key(14)]
	public int[] ConditionValue3 { get; set; }

	PlayerArchiveDataKey IDataKeySelector<PlayerArchiveDataKey>.Key => new PlayerArchiveDataKey(MainIndex, SubIndex);

	public override string ToString()
	{
		return "MainIndex=" + MainIndex + "SubIndex" + SubIndex;
	}
}
