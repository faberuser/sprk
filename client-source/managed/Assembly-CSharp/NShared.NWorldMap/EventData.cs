using MessagePack;

namespace NShared.NWorldMap;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class EventData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ChapterIndex { get; set; }

	[Key(2)]
	public EventType EventType { get; set; }

	[Key(3)]
	public int[] DungeonIndex { get; set; }

	[Key(4)]
	public bool CompletedDungeonOnly { get; set; }

	[Key(5)]
	public bool CurrentChapterOnly { get; set; }

	[Key(6)]
	public int ReqChapterIndex { get; set; }

	[Key(7)]
	public int ReqDungeonIndex { get; set; }

	[Key(8)]
	public int[] EventValue { get; set; }

	[Key(9)]
	public int OccurCount { get; set; }

	[Key(10)]
	public int[] OccurPercentage { get; set; }

	[Key(11)]
	public int[] OccurHour { get; set; }

	[Key(12)]
	public int Duration { get; set; }

	[Key(13)]
	public int Cooltime { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
