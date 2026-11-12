using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CreatureTranscendOptionData : IDataKeySelector<int>
{
	[Key(0)]
	public int TranscendLevel { get; set; }

	[Key(1)]
	public int SlotIndex { get; set; }

	[Key(2)]
	public int ReqTranscendPoint { get; set; }

	[Key(3)]
	public int MaxLevel { get; set; }

	[Key(4)]
	public string IconPath { get; set; }

	[Key(5)]
	public OperationType OptionType1 { get; set; }

	[Key(6)]
	public int OptionValue1 { get; set; }

	[Key(7)]
	public OperationType OptionType2 { get; set; }

	[Key(8)]
	public int OptionValue2 { get; set; }

	[Key(9)]
	public int LegacySkillIndex { get; set; }

	int IDataKeySelector<int>.Key => TranscendLevel * 10 + SlotIndex;

	public override string ToString()
	{
		return $"Key=({TranscendLevel}, {SlotIndex})";
	}
}
